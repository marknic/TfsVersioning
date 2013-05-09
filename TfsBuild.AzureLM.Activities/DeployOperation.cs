using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.TeamFoundation.Build.Client;
using TfsBuild.AzureLM.Activities.BlobTransfer;
using TfsBuild.AzureLM.Activities.Contracts;
using TfsBuild.AzureLM.Activities.Models;
using TfsBuild.AzureLM.Activities.Utility;
using TfsBuild.AzureLm.Activities.Contracts;
using TfsBuild.AzureLm.Activities.Enumerations;
using TfsBuild.AzureLm.Activities.Models;
using TfsBuild.AzureLm.Activities.Utility;

// ==============================================================================================
// AzureLM - Azure Lifecycle Management Extension for Team Foundation Server
//
// Author: Mark S. Nichols
//
// Copyright (c) 2013 Microsoft Corporation
//
// ==============================================================================================

namespace TfsBuild.AzureLm.Activities
{
    public class DeployOperation : IDeployOperation
    {
        #region Properties

        public int PollInterval { get; set; }
        public int PollTimeout { get; set; }
        public string SubscriptionId { get; private set; }
        public string HostedServiceName { get; private set; }
        public string StorageServiceName { get; private set; }
        public string HostedServiceLocation { get; private set; }

        public CodeActivityContext CodeActivityContext { get; private set; }

        
        // This is the common namespace for all Service Management REST API XML data.
        private static readonly XNamespace WindowsAzureSchema = Constants.WindowsAzureSchema;

        /// <summary>
        /// Gets or sets the certificate that matches the Thumbprint value.
        /// </summary>
        private ICertificateManager CertManager { get; set; }

        #endregion

        /// <summary>
        /// Execute the upload and deployment of a Windows Azure Package
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="hostedServiceName">Service name (AKA DNS prefix)</param>
        /// <param name="certificateThumbprint">Thumbprint of the certificate used to manage the Windows Azure environment</param>
        /// <param name="deployName">Name to give the deployment</param>
        /// <param name="deployLabel">Detailed name to give the deployment.  This is the text seen in the management portal</param>
        /// <param name="storageServiceName">Storage account (blob storage) to use for the deployment.  The package will be copied here for deployment.</param>
        /// <param name="configurationFilepath">Local filepath of the configuration (cscfg) file to use during the deployment</param>
        /// <param name="packageFilepath">Local filepath of the package to be deployed.</param>
        /// <param name="storageContainer">Name of the blob container to store the package for deployment</param>
        /// <param name="blobStorageFolder">Name of the folder within the blob container to store the package for deployment</param>
        /// <param name="startAfterDeploy">True if the deployment is to be started following the deployment</param>
        /// <param name="waitForDeployCompletion">Set to false if the build process is not to wait for the deploymen to complete before returning.
        /// This will speed up the build process.  Note: if a VIP Swap (staging to production) is required, then this flag will be ignored.</param>
        /// <param name="context">Build activity context</param>
        /// <param name="deploymentWorkflow">Defines the workflow to take place when a package is to be deployed (delete, demote and promote protocols)</param>
        public void Execute(string subscriptionId, string hostedServiceName, string certificateThumbprint, string deployName, string deployLabel,
            string storageServiceName, string configurationFilepath, string packageFilepath, string storageContainer, string blobStorageFolder,
            bool startAfterDeploy, bool waitForDeployCompletion, CodeActivityContext context, DeploymentWorkflowTargets deploymentWorkflow = DeploymentWorkflowTargets.StagingNoDelete)
        {
            if (deploymentWorkflow == DeploymentWorkflowTargets.None)
            {
                // No work to be done
                WriteBuildMessage("Deployment Workflow set to None.  No deployment action will be taken.");

                return;
            }

            #region Parameter Validation

            if (string.IsNullOrWhiteSpace(deployName))
            {
                throw new ArgumentNullException("deployName");
            }
            if (string.IsNullOrWhiteSpace(deployLabel))
            {
                throw new ArgumentNullException("deployLabel");
            }
            if (string.IsNullOrWhiteSpace(certificateThumbprint))
            {
                throw new ArgumentNullException("certificateThumbprint");
            }
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                throw new ArgumentNullException("subscriptionId");
            }
            if (string.IsNullOrWhiteSpace(hostedServiceName))
            {
                throw new ArgumentNullException("hostedServiceName");
            }
            if (string.IsNullOrWhiteSpace(storageServiceName))
            {
                throw new ArgumentNullException("storageServiceName");
            }
            if (string.IsNullOrWhiteSpace(configurationFilepath))
            {
                throw new ArgumentNullException("configurationFilepath");
            }
            if (string.IsNullOrWhiteSpace(packageFilepath))
            {
                throw new ArgumentNullException("packageFilepath");
            }
            if (!File.Exists(configurationFilepath))
            {
                throw new ArgumentException(string.Format("The Configuration File does not exist in the location provided: {0}", configurationFilepath));
            }
            if (!File.Exists(packageFilepath))
            {
                throw new ArgumentException(string.Format("The Package File does not exist in the location provided: {0}", packageFilepath));
            }

            #endregion

            #region Set Properties and load management certificate

            PollingResult pollingResult;

            PollInterval = 10; // 10 second polling
            PollTimeout = 180; // 3 minutes

            HostedServiceName = hostedServiceName;
            SubscriptionId = subscriptionId;
            CodeActivityContext = context;

            // Create the cert manager and load the certificate
            CertManager = CertificateManager.CreateCertificateManager(certificateThumbprint);

            // Step 1: Get and verify the management certificate
            WriteBuildMessage("Local management certificate successfully loaded");

            // Check Azure to see if the corresponding certificate exists
            GetCertificateProperties(SubscriptionId, certificateThumbprint);

            WriteBuildMessage("Azure management certificate existance verified.");

            #endregion

            #region Get Properties Document from Azure

            // Step 2: Get the existing deployment properties of the deployed services

            var stagingExists = false;
            var productionExists = false;

            DoProductionAndStagingExist(ref stagingExists, ref productionExists);


            // Step 3. Set up blob storage to hold the deployment and upload the package

            var blobUri = UploadBlobAndRetrieveUri(storageServiceName, packageFilepath, storageContainer);

            #endregion

            // Step 4. Decode intent of the workflow
            var workflowIndicators = DeploymentSupport.DecodeWorkflowData(deploymentWorkflow, stagingExists, productionExists);

            if (deploymentWorkflow == DeploymentWorkflowTargets.UploadOnly)
            {
                // There's nothing to do so report and return
                WriteBuildMessage("Workflow is UploadOnly - No deployment steps will be taken.");

                return;
            }

            // Step 5. Delete the existing deployment if there is one in the destination slot
            if (workflowIndicators.DoDelete)
            {
                WriteBuildMessage("Deleting existing deployment in slot " + workflowIndicators.DeploymentSlot);

                var deleteDeploymentResult = DeleteDeployment(SubscriptionId, HostedServiceName, workflowIndicators.DeploymentSlot);

                pollingResult = PollGetOperationStatus(SubscriptionId, deleteDeploymentResult.RequestId, PollInterval, PollTimeout);

                DecodeResult(deleteDeploymentResult, pollingResult);

                WriteBuildMessage("Existing deployment in slot '" + workflowIndicators.DeploymentSlot + "' deleted.");
            }
            else
            {
                WriteBuildMessage("No deployment exists in " + workflowIndicators.DeploymentSlot + ". No deletion.");
            }
        

            // Step 6: Deploy package in blob storage to the slot
            WriteBuildMessage("Deploying to slot: " + workflowIndicators.DeploymentSlot);

            var createDeploymentResult = CreateDeployment(SubscriptionId, HostedServiceName, workflowIndicators.DeploymentSlot, deployLabel, deployName,
                                                blobUri.ToString(), configurationFilepath, startAfterDeploy);

            // Should we return now even though the deployment has not completed?
            if ((waitForDeployCompletion == false) && (workflowIndicators.DoSwap == false))
            {
                WriteBuildMessage("Deployment was initiated.  WaitForDeployCompletion flag was set to false.  Exiting immediately.");
                return; 
            }

            pollingResult = PollGetOperationStatus(SubscriptionId, createDeploymentResult.RequestId, PollInterval, PollTimeout);

            DecodeResult(createDeploymentResult, pollingResult);

            WriteBuildMessage("Deployment to '" + workflowIndicators.DeploymentSlot + "' succeeded.");

 
            // Step 7: VIP Swap
            if (!workflowIndicators.DoSwap) { return; }

            WriteBuildMessage("Performing VIP Swap");

            // Get the existing properties of the deployed services
            var hostedServicePropertiesResult = GetHostedServiceProperties(SubscriptionId, HostedServiceName);

            var deploymentInfoList = DeploymentInfo.GetDeploymentList(hostedServicePropertiesResult.ResponseBody);
                    
            // VIP Swap the deployments
            var swapResult = SwapDeployment(SubscriptionId, HostedServiceName, deploymentInfoList);

            pollingResult = PollGetOperationStatus(SubscriptionId, swapResult.RequestId, PollInterval, PollTimeout);

            DecodeResult(createDeploymentResult, pollingResult);

            WriteBuildMessage("Deployment to '" + workflowIndicators.DeploymentSlot + "' succeeded.");
        }

        private Uri UploadBlobAndRetrieveUri(string storageServiceName, string packageFilepath, string storageContainer)
        {
            // If the storage service name was not provided then create our own
            StorageServiceName = string.IsNullOrWhiteSpace(storageServiceName)
                                     ? Constants.AzureStorageServiceDefaultName
                                     : storageServiceName;

            // Look for the storage account - if it's there great, if not then need to create one
            var storageResults = GetStorageAccountProperties(SubscriptionId, StorageServiceName);

            if (!storageResults.ResultCodeMatch)
            {
                WriteBuildMessage("Storage account properties were not found.  Creating deployment blob storage.");
                // Storage account was not found so create one
                var createStorageResponse = CreateStorageAccount(SubscriptionId, StorageServiceName,
                                                                 Constants.StorageDescription,
                                                                 Constants.StorageLabel, null, HostedServiceLocation);

                var pollingResult = PollGetOperationStatus(SubscriptionId, storageResults.RequestId, PollInterval, PollTimeout);

                DecodeResult(createStorageResponse, pollingResult);
            }


            WriteBuildMessage("Retrieving storage account key.");

            // Get storage key
            var storageKeyResults = GetStorageKey(SubscriptionId, StorageServiceName);

            var storageKeyElement =
                storageKeyResults.ResponseBody.Descendants(WindowsAzureSchema + AzureApiElementNames.PrimaryKey)
                                 .FirstOrDefault();

            if (storageKeyElement == null)
            {
                throw new ApplicationException("Failed to retrieve storage key from Azure. No primary key was returned.");
            }

            // Create storage information object to pass on to the upload
            var storageData = new Storage(storageServiceName, storageContainer, storageKeyElement.Value);

            // Upload package to blob storage
            WriteBuildMessage("Uploading '" + packageFilepath + "' to blob storage for deployment.");

            var blobUri = BlobTransfer.UploadBlob(storageData, packageFilepath);

            WriteBuildMessage("Package uploaded: " + blobUri);

            return blobUri;
        }


        private void DoProductionAndStagingExist(ref bool stagingExists, ref bool productionExists)
        {
            var hostedServicePropertiesResult = GetHostedServiceProperties(SubscriptionId, HostedServiceName);

            // Extract the location of the hosted service just in case we need to create a storage account

            var serviceLocationElement =
                hostedServicePropertiesResult.ResponseBody.Descendants(WindowsAzureSchema + AzureApiElementNames.Location)
                                             .FirstOrDefault();


            // Extract the pertinent existing deployment information from the document
            var deploymentInfoList = DeploymentInfo.GetDeploymentList(hostedServicePropertiesResult.ResponseBody);

            foreach (var deploymentInfo in deploymentInfoList)
            {
                if (deploymentInfo.Slot == DeploymentSlots.Staging)
                {
                    stagingExists = true;
                    WriteBuildMessage("Staging Slot Full");
                }

                if (deploymentInfo.Slot != DeploymentSlots.Production) continue;

                productionExists = true;
                WriteBuildMessage("Production Slot Full");
            }

            if (serviceLocationElement != null)
            {
                // location value discovered
                HostedServiceLocation = serviceLocationElement.Value;
            }
            else
            {
                throw new ApplicationException(
                    "Retrieval of Hosted Service information returned null. Verify the name provided is a valid Azure hosted service name: " +
                    HostedServiceName);
            }
        }


        /// <summary>
        /// Retrieves certificate information from Azure.  The real purpose here is to confirm that a corresponding 
        /// management certificate exists.  If it does, then the rest of the deployment operations can continue.
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="thumbprint">Certificate thumbprint</param>
        /// <returns>Results of the management api call</returns>
        public ManagementApiResults GetCertificateProperties(string subscriptionId, string thumbprint)
        {
            const string uriFormat = Constants.UriBase + "/certificates/{1}";

            var uri = new Uri(String.Format(uriFormat, subscriptionId, thumbprint));

            var apiResponse = InvokeRequest(uri, "GET", AzureApiVersions.Version2012_03_01, HttpStatusCode.OK, null, false);

            if (!apiResponse.ResultCodeMatch)
            {
                throw new ApplicationException(
                    "The management certificate " + thumbprint + " was not found in Azure.  Verify that the local and Azure based certificates match. Status Code: " + apiResponse.StatusCode);
            }

            return apiResponse;
        }


        /// <summary>
        /// Retrieves the storage keys from Azure
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="storageServiceName">Name of the Azure storage service</param>
        /// <returns>ManagementApiResults - internal standard api results object</returns>
        public ManagementApiResults GetStorageKey(string subscriptionId, string storageServiceName)
        {
            const string uriFormat = Constants.UriBase + "/services/storageservices/{1}/keys";

            var uri = new Uri(String.Format(uriFormat, subscriptionId, storageServiceName));

            var apiResponse = InvokeRequest(uri, "GET", AzureApiVersions.Version2011_10_01, HttpStatusCode.OK, null, false);

            if (!apiResponse.ResultCodeMatch)
            {
                throw new ApplicationException("Failed to retrieve storage keys. Status Code: " + apiResponse.StatusCode);
            }

            return apiResponse;
        }

        /// <summary>
        /// Retrieves information about the Azure hosted service
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="hostedServiceName">Name of the Azure hosted service</param>
        /// <returns>ManagementApiResults - internal standard api results object</returns>
        public ManagementApiResults GetHostedServiceProperties(string subscriptionId, string hostedServiceName)
        {
            if (string.IsNullOrWhiteSpace(hostedServiceName)) throw new ArgumentNullException("hostedServiceName");

            const string uriFormat = Constants.UriBase + "/services/hostedservices/{1}";

            var uri = new Uri(String.Format(uriFormat, subscriptionId, hostedServiceName));

            var apiResponse = InvokeRequest(uri, "GET", "2012-03-01", HttpStatusCode.OK, null);

            if (!apiResponse.ResultCodeMatch)
            {
                throw new ApplicationException("Failed to retrieve Hosted Service Properties. Status Code: " + apiResponse.StatusCode);
            }

            return apiResponse;
        }





        /// <summary>
        /// Polls Get Operation Status for the operation specified by requestId
        /// every pollIntervalSeconds until timeoutSeconds have passed or the
        /// operation has returned a Failed or Succeeded status. 
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="requestId">The requestId of the operation to get status for.</param>
        /// <param name="pollIntervalSeconds">The interval between calls to Get Operation Status.</param>
        /// <param name="timeoutSeconds">The maximum number of seconds to poll.</param>
        /// <returns>An PollingResult structure with status or error information.</returns>
        private PollingResult PollGetOperationStatus(string subscriptionId, string requestId, int pollIntervalSeconds, int timeoutSeconds)
        {
            var result = new PollingResult();
            var beginPollTime = DateTime.UtcNow;
            var pollInterval = new TimeSpan(0, 0, pollIntervalSeconds);
            var endPollTime = beginPollTime + new TimeSpan(0, 0, timeoutSeconds);

            var done = false;

            while (!done)
            {
                var operation = GetOperationStatus(subscriptionId, requestId);

                result.RunningTime = DateTime.UtcNow - beginPollTime;

                try
                {
                    // Turn the Status string into an OperationStatus value
                    var xElement = operation.Element(WindowsAzureSchema + "Status");

                    if (xElement != null)
                    {
                        result.Status = (AzureApiOperationStatus)Enum.Parse(typeof(AzureApiOperationStatus), xElement.Value);
                    }
                }
                catch (Exception)
                {
                    throw new ApplicationException(string.Format("Get Operation Status {0} returned unexpected status: {1}{2}", requestId, Environment.NewLine,
                        operation.ToString(SaveOptions.OmitDuplicateNamespaces)));
                }

                switch (result.Status)
                {
                    case AzureApiOperationStatus.InProgress:
                        Thread.Sleep((int)pollInterval.TotalMilliseconds);

                        break;

                    case AzureApiOperationStatus.Failed:
                        var xElement = operation.Element(WindowsAzureSchema + AzureApiElementNames.HttpStatusCode);

                        if (xElement != null)
                        {
                            result.StatusCode = (HttpStatusCode)Convert.ToInt32(xElement.Value);
                        }

                        var error = operation.Element(WindowsAzureSchema + AzureApiElementNames.Error);

                        if (error != null)
                        {
                            var codeElement = error.Element(WindowsAzureSchema + AzureApiElementNames.Code);
                            if (codeElement != null)
                            {
                                result.Code = codeElement.Value;
                            }

                            var messageElement = error.Element(WindowsAzureSchema + AzureApiElementNames.Message);
                            if (messageElement != null)
                            {
                                result.Message = messageElement.Value;
                            }
                        }
                        done = true;

                        break;

                    case AzureApiOperationStatus.Succeeded:
                        var element = operation.Element(WindowsAzureSchema + AzureApiElementNames.HttpStatusCode);
                        if (element != null)
                        {
                            result.StatusCode = (HttpStatusCode)Convert.ToInt32(element.Value);
                        }

                        done = true;
                        break;
                }

                if (done || DateTime.UtcNow <= endPollTime) continue;

                result.Status = AzureApiOperationStatus.TimedOut;
                done = true;
            }

            return result;
        }

        /// <summary>
        /// Calls the Get Operation Status operation in the Service 
        /// Management REST API for the specified subscription and requestId 
        /// and returns the Operation XML element from the response.
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="requestId">The requestId of the operation to track.</param>
        /// <returns>The Operation XML element from the response.</returns>
        private XElement GetOperationStatus(string subscriptionId, string requestId)
        {
            const string uriFormat = Constants.UriBase + "/operations/{1}";

            var uri = new Uri(String.Format(uriFormat, subscriptionId, requestId));

            var response = InvokeRequest(uri, "GET", AzureApiVersions.Version2012_03_01, HttpStatusCode.OK, null);

            return response.ResponseBody.Element(WindowsAzureSchema + "Operation");
        }

        /// <summary>
        /// A helper function to invoke a Service Management REST API operation.
        /// Throws an ApplicationException on unexpected status code results.
        /// </summary>
        /// <param name="uri">The URI of the operation to invoke using a web request.</param>
        /// <param name="method">The method of the web request, GET, PUT, POST, or DELETE.</param>
        /// <param name="version"></param>
        /// <param name="expectedCode">The expected status code.</param>
        /// <param name="requestBody">The XML body to send with the web request. Use null to send no request body.</param>
        /// <param name="throwExceptionOnError">true if the method should throw an exception if the expected code and resulting code do not match</param>
        /// <returns>The requestId returned by the operation and the response body in a ManagementCallResults object.</returns>
        private ManagementApiResults InvokeRequest(Uri uri, string method, string version, HttpStatusCode expectedCode,
            XDocument requestBody, bool throwExceptionOnError = true)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (string.IsNullOrWhiteSpace(method)) throw new ArgumentNullException("method");
            if (string.IsNullOrWhiteSpace(version)) throw new ArgumentNullException("version");

            XDocument responseBody = null;
            var requestId = String.Empty;

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.Headers.Add("x-ms-Version", version);
            request.ClientCertificates.Add(CertManager.Certificate);
            request.ContentType = "application/xml";

            if (requestBody != null)
            {
                Stream requestStream = null;

                try
                {
                    requestStream = request.GetRequestStream();

                    using (var streamWriter = new StreamWriter(
                        requestStream, Encoding.UTF8))
                    {
                        requestStream = null;
                        requestBody.Save(streamWriter, SaveOptions.DisableFormatting);
                    }
                }
                finally
                {
                    if (requestStream != null)
                        requestStream.Dispose();
                }

            }

            HttpWebResponse response;
            HttpStatusCode statusCode;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    // GetResponse throws a WebException for 4XX and 5XX status codes
                    response = (HttpWebResponse)ex.Response;
                }
                else
                {
                    throw new ArgumentException(ex.Message + " - Check to see if the management certificate exists in Azure. Cannot proceed.");
                }
            }

            try
            {
                statusCode = response.StatusCode;
                if (response.ContentLength > 0)
                {
                    if (response != null)
                    {
                        Stream stream = null;

                        try
                        {
                            stream = response.GetResponseStream();

                            if (stream != null)
                            {
                                using (var reader = XmlReader.Create(stream))
                                {
                                    stream = null;

                                    responseBody = XDocument.Load(reader);
                                }
                            }
                        }
                        finally
                        {
                            if (stream != null)
                                stream.Dispose();
                        }
                    }
                }

                if (response.Headers != null)
                {
                    requestId = response.Headers["x-ms-request-id"];
                }
            }
            finally
            {
                response.Close();
            }

            var resultMatch = statusCode.Equals(expectedCode);

            if ((!resultMatch) && (throwExceptionOnError))
            {
                var message = responseBody != null
                                  ? responseBody.ToString(SaveOptions.OmitDuplicateNamespaces)
                                  : "Unknown error: The response was not received properly";

                throw new ApplicationException(
                    string.Format("Call to {0} returned an error:{1}Status Code: {2} ({3}):{1}{4}", uri,
                                  Environment.NewLine,
                                  (int)statusCode, statusCode, message));

            }

            return new ManagementApiResults
            {
                RequestId = requestId,
                ResponseBody = responseBody,
                ResultCodeMatch = resultMatch,
                StatusCode = statusCode
            };
        }


        /// <summary>
        /// Calls the Delete Storage Account operation in the Service Management
        /// REST API for the specified subscription and storage account name.
        /// Throws an ApplicationException on status code results other than OK.
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="serviceName">Service name (AKA DNS prefix)</param>
        /// <param name="slotToDelete">The slot in which the deployment is to be deleted</param>
        /// <returns>Result of the deletion</returns>
        public ManagementApiResults DeleteDeployment(string subscriptionId, string serviceName, DeploymentSlots slotToDelete)
        {
            const string uriFormat = Constants.UriBase + "/services/hostedservices/{1}/deploymentslots/{2}";
        
            var uri = new Uri(String.Format(uriFormat, subscriptionId, serviceName, slotToDelete));

            var apiResult = InvokeRequest(uri, HttpVerbs.DELETE.ToString(), AzureApiVersions.Version2009_10_01, HttpStatusCode.Accepted, null, false);

            if (!apiResult.ResultCodeMatch)
            {
                throw new ApplicationException(
                    "Delete deployment failed: API call returned code: " + apiResult.StatusCode);
            }

            return apiResult;
        }

        /// <summary>
        /// Utility method to validate the polled service
        /// </summary>
        /// <param name="apiResult">Results from the original API call</param>
        /// <param name="pollingResult">Results from status polling</param>
        private void DecodeResult(ManagementApiResults apiResult, PollingResult pollingResult)
        {
            switch (pollingResult.Status)
            {
                case AzureApiOperationStatus.TimedOut:
                    throw new ApplicationException(
                        string.Format(
                            "Poll of Get Operation Status timed out: Operation {0} is still in progress after {1} seconds.",
                            apiResult.RequestId,
                            (int)pollingResult.RunningTime.TotalSeconds));

                case AzureApiOperationStatus.Failed:
                    throw new ApplicationException(string.Format(
                        "Failed: Operation {0} failed after {1} seconds with status {2} ({3}) - {4}: {5}",
                        apiResult.RequestId,
                        (int)pollingResult.RunningTime.TotalSeconds,
                        (int)pollingResult.StatusCode,
                        pollingResult.StatusCode,
                        pollingResult.Code,
                        pollingResult.Message));

                case AzureApiOperationStatus.Succeeded:
                    WriteBuildMessage(string.Format(
                        "Succeeded: Operation {0} completed after {1} seconds with status {2} ({3})",
                        apiResult.RequestId,
                        (int) pollingResult.RunningTime.TotalSeconds,
                        (int) pollingResult.StatusCode,
                        pollingResult.StatusCode));
                    break;
            }
        }


        /// <summary>
        /// Swap existing hosted services
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="hostedServiceName">Service name (AKA DNS prefix)</param>
        /// <param name="deploymentInfoList">List of DeploymentInfo objects that define the VIP Swap to be performed</param>
        /// <returns>Result of the services swap</returns>
        public ManagementApiResults SwapDeployment(string subscriptionId, string hostedServiceName, List<DeploymentInfo> deploymentInfoList)
        {
            if (deploymentInfoList.Count != 2)
            {
                throw new ArgumentException("DeploymentInfoList does not contain staging and production slots.");
            }

            const string uriFormat = Constants.UriBase + "/services/hostedservices/{1}";
            
            var uri =
                new Uri(
                    String.Format(uriFormat, subscriptionId, hostedServiceName));

            string productionName = null;
            string stagingName = null;

            foreach (var deploymentInfo in deploymentInfoList)
            {
                if (deploymentInfo.Slot == DeploymentSlots.Production)
                {
                    productionName = deploymentInfo.Name;
                }
                else
                {
                    stagingName = deploymentInfo.Name;
                }
            }

            if (string.IsNullOrWhiteSpace(productionName) || string.IsNullOrWhiteSpace(stagingName))
            {
                throw new ArgumentException("DeploymentInfoList did not contain staging and production slot names");
            }

            var swapDeploymentDocument = new XDocument(
                new XElement(Constants.WindowsAzureSchema + "Swap",
                             new XElement(Constants.WindowsAzureSchema + "Production", productionName),
                             new XElement(Constants.WindowsAzureSchema + "SourceDeployment", stagingName)
                    )
                ) { Declaration = new XDeclaration("1.0", "utf-8", "yes") };

            var invokeResult = InvokeRequest(uri, HttpVerbs.POST.ToString(), AzureApiVersions.Version2009_10_01, HttpStatusCode.Accepted, swapDeploymentDocument);

            if (!invokeResult.ResultCodeMatch)
            {
                throw new ApplicationException("Attempt to initiate a VIP Swap failed. Status Code: " + invokeResult.StatusCode);
            }
 
            return invokeResult;
        }


        /// <summary>
        /// Creates a deployment given a package, configuration and destination
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="hostedServiceName">Service name (AKA DNS prefix)</param>
        /// <param name="slotToDeploy">Destination slot for the deployment (Production or Staging)</param>
        /// <param name="deploymentLabel">Label description for this deployment</param>
        /// <param name="deploymentName">Name for the deployment</param>
        /// <param name="packageUrl">Location of the deployment package residing in blob storage (cspkg file)</param>
        /// <param name="configurationFilepath">Local filepath for the deployment configuration (cscfg file)</param>
        /// <param name="startAfterDeploy">True if the deployment is to be started following the deployment</param>
        /// <returns>DeploymentStepResult object with success or failure information</returns>
        public ManagementApiResults CreateDeployment(string subscriptionId, string hostedServiceName, DeploymentSlots slotToDeploy,
            string deploymentLabel, string deploymentName, string packageUrl, string configurationFilepath, bool startAfterDeploy)
        {
            if (string.IsNullOrWhiteSpace(hostedServiceName)) { throw new ArgumentNullException("hostedServiceName"); }
            if (string.IsNullOrWhiteSpace(deploymentLabel)) { throw new ArgumentNullException("deploymentLabel"); }
            if (string.IsNullOrWhiteSpace(deploymentName)) { throw new ArgumentNullException("deploymentName"); }
            if (string.IsNullOrWhiteSpace(packageUrl)) { throw new ArgumentNullException("packageUrl"); }
            if (string.IsNullOrWhiteSpace(configurationFilepath)) { throw new ArgumentNullException("configurationFilepath"); }

            const string uriFormat = Constants.UriBase + "/services/hostedservices/{1}/deploymentslots/{2}";
            
            var uri =
                new Uri(
                    String.Format(uriFormat, subscriptionId, hostedServiceName, slotToDeploy));

            if (!File.Exists(configurationFilepath)) { throw new ArgumentException("Configuration file: '" + configurationFilepath + "' does not exist."); }

            var configurationData = File.ReadAllText(configurationFilepath);

            var createDeploymentDocument = new XDocument(
                new XElement(Constants.WindowsAzureSchema + "CreateDeployment",
                             new XElement(Constants.WindowsAzureSchema + "Name", deploymentName),
                             new XElement(Constants.WindowsAzureSchema + "PackageUrl", packageUrl),
                             new XElement(Constants.WindowsAzureSchema + "Label", DeploymentSupport.EncodeToBase64String(deploymentLabel)),
                             new XElement(Constants.WindowsAzureSchema + "Configuration", DeploymentSupport.EncodeToBase64String(configurationData)),
                             new XElement(Constants.WindowsAzureSchema + "StartDeployment", startAfterDeploy.ToString(CultureInfo.InvariantCulture).ToLower()),
                             new XElement(Constants.WindowsAzureSchema + "TreatWarningsAsError", "true")
                    )
                ) { Declaration = new XDeclaration("1.0", "utf-8", "yes") };

            var invokeResult = InvokeRequest(uri, HttpVerbs.POST.ToString(), AzureApiVersions.Version2012_03_01, HttpStatusCode.Accepted, createDeploymentDocument);

            if (!invokeResult.ResultCodeMatch)
            {
                throw new ApplicationException("Attempt to deploy package to '" + slotToDeploy + "' failed.");
            }

            return invokeResult;
        }


        /// <summary>
        /// Calls the Get Storage Account Properties operation in the Service 
        /// Management REST API for the specified subscription and storage account 
        /// name and returns the StorageService XML element from the response.
        /// </summary>
        /// <param name="subscriptionId">Azure subscription ID (GUID)</param>
        /// <param name="storageServiceName">The name of the storage account.</param>
        /// <returns>The StorageService XML element from the response.</returns>
        public ManagementApiResults GetStorageAccountProperties(string subscriptionId, string storageServiceName)
        {
            const string uriFormat = Constants.UriBase + "/services/storageservices/{1}";

            var uri = new Uri(String.Format(uriFormat, subscriptionId, storageServiceName));

            var apiResponse = InvokeRequest(uri, "GET", AzureApiVersions.Version2011_10_01, HttpStatusCode.OK, null, false);

            if (!apiResponse.ResultCodeMatch)
            {
                Console.WriteLine("Existing storage account properties were not found.");
            }

            return apiResponse;
        }


      
        /// <summary>
        /// Calls the Create Storage Account operation in the Service Management 
        /// REST API for the specified subscription, storage account name, 
        /// description, label, and location or affinity group.
        /// </summary>
        /// <param name="subscriptionId">Azure subscription ID (GUID)</param>
        /// <param name="storageServiceName">The name of the storage account to update.</param>
        /// <param name="description">The new description for the storage account.</param>
        /// <param name="label">The new label for the storage account.</param>
        /// <param name="affinityGroup">The affinity group name, or null to use a location.</param>
        /// <param name="location">The location name, or null to use an affinity group.</param>
        /// <returns>The requestId for the operation.</returns>
        public ManagementApiResults CreateStorageAccount(string subscriptionId, string storageServiceName, string description,
            string label, string affinityGroup, string location)
        {
            const string uriFormat = Constants.UriBase + "/services/storageservices";

            var uri = new Uri(String.Format(uriFormat, subscriptionId));

            // Location and Affinity Group are mutually exclusive. 
            // Use the location if it isn't null or empty.
            var locationOrAffinityGroup = String.IsNullOrEmpty(location) ?
                new XElement(WindowsAzureSchema + "AffinityGroup", affinityGroup) :
                new XElement(WindowsAzureSchema + "Location", location);

            // Create the request XML document
            var requestBody = new XDocument(
                new XDeclaration("1.0", "UTF-8", "no"),
                new XElement(
                    WindowsAzureSchema + "CreateStorageServiceInput",
                    new XElement(WindowsAzureSchema + "ServiceName", storageServiceName),
                    new XElement(WindowsAzureSchema + "Description", description),
                    new XElement(WindowsAzureSchema + "Label", DeploymentSupport.EncodeToBase64String(label)),
                    locationOrAffinityGroup));

            return InvokeRequest(
                uri, "POST", AzureApiVersions.Version2012_03_01, HttpStatusCode.Accepted, requestBody);
        }


        private void WriteBuildMessage(string message)
        {
            if (CodeActivityContext != null)
            {
                CodeActivityContext.WriteBuildMessage(message, BuildMessageImportance.High);
            }
        }
    }
}
