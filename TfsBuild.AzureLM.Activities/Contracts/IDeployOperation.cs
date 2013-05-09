using System.Activities;
using System.Collections.Generic;
using TfsBuild.AzureLM.Activities.Models;
using TfsBuild.AzureLm.Activities.Enumerations;
using TfsBuild.AzureLm.Activities.Models;

// ==============================================================================================
// AzureLM - Azure Lifecycle Management Extension for Team Foundation Server
//
// Author: Mark S. Nichols
//
// Copyright (c) 2013 Microsoft Corporation
//
// ==============================================================================================

namespace TfsBuild.AzureLM.Activities.Contracts
{
    public interface IDeployOperation
    {
        int PollInterval { get; set; }
        int PollTimeout { get; set; }
        string SubscriptionId { get; }
        string HostedServiceName { get; }
        string StorageServiceName { get; }
        string HostedServiceLocation { get; }
        CodeActivityContext CodeActivityContext { get; }

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
        void Execute(string subscriptionId, string hostedServiceName, string certificateThumbprint, string deployName, string deployLabel,
                                     string storageServiceName, string configurationFilepath, string packageFilepath, string storageContainer, string blobStorageFolder,
                                     bool startAfterDeploy, bool waitForDeployCompletion, CodeActivityContext context, DeploymentWorkflowTargets deploymentWorkflow = DeploymentWorkflowTargets.StagingNoDelete);

        /// <summary>
        /// Retrieves certificate information from Azure.  The real purpose here is to confirm that a corresponding 
        /// management certificate exists.  If it does, then the rest of the deployment operations can continue.
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="thumbprint">Certificate thumbprint</param>
        /// <returns>Results of the management api call</returns>
        ManagementApiResults GetCertificateProperties(string subscriptionId, string thumbprint);

        /// <summary>
        /// Retrieves the storage keys from Azure
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="storageServiceName">Name of the Azure storage service</param>
        /// <returns>ManagementApiResults - internal standard api results object</returns>
        ManagementApiResults GetStorageKey(string subscriptionId, string storageServiceName);

        /// <summary>
        /// Retrieves information about the Azure hosted service
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="hostedServiceName">Name of the Azure hosted service</param>
        /// <returns>ManagementApiResults - internal standard api results object</returns>
        ManagementApiResults GetHostedServiceProperties(string subscriptionId, string hostedServiceName);

        /// <summary>
        /// Calls the Delete Storage Account operation in the Service Management
        /// REST API for the specified subscription and storage account name.
        /// Throws an ApplicationException on status code results other than OK.
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="serviceName">Service name (AKA DNS prefix)</param>
        /// <param name="slotToDelete">The slot in which the deployment is to be deleted</param>
        /// <returns>Result of the deletion</returns>
        ManagementApiResults DeleteDeployment(string subscriptionId, string serviceName, DeploymentSlots slotToDelete);

        /// <summary>
        /// Swap existing hosted services
        /// </summary>
        /// <param name="subscriptionId">Windows Azure subscription ID</param>
        /// <param name="hostedServiceName">Service name (AKA DNS prefix)</param>
        /// <param name="deploymentInfoList">List of DeploymentInfo objects that define the VIP Swap to be performed</param>
        /// <returns>Result of the services swap</returns>
        ManagementApiResults SwapDeployment(string subscriptionId, string hostedServiceName, List<DeploymentInfo> deploymentInfoList);

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
        ManagementApiResults CreateDeployment(string subscriptionId, string hostedServiceName, DeploymentSlots slotToDeploy,
                                                              string deploymentLabel, string deploymentName, string packageUrl, string configurationFilepath, bool startAfterDeploy);

        /// <summary>
        /// Calls the Get Storage Account Properties operation in the Service 
        /// Management REST API for the specified subscription and storage account 
        /// name and returns the StorageService XML element from the response.
        /// </summary>
        /// <param name="subscriptionId">Azure subscription ID (GUID)</param>
        /// <param name="storageServiceName">The name of the storage account.</param>
        /// <returns>The StorageService XML element from the response.</returns>
        ManagementApiResults GetStorageAccountProperties(string subscriptionId, string storageServiceName);

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
        ManagementApiResults CreateStorageAccount(string subscriptionId, string storageServiceName, string description,
                                                                  string label, string affinityGroup, string location);
    }
}