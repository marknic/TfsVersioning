using System;
using System.Activities;
using System.Drawing;
using Microsoft.TeamFoundation.Build.Client;
using TfsBuild.AzureLM.Activities.Contracts;
using TfsBuild.AzureLm.Activities.Enumerations;
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

    [ToolboxBitmap(typeof(AzureDeploy), "Resources.azurelm.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class AzureDeploy : CodeActivity
    {
        #region Workflow Parameters

        [RequiredArgument]
        public InArgument<string> SubscriptionId { get; set; }

        [RequiredArgument]
        public InArgument<string> ServiceName { get; set; }

        [RequiredArgument]
        public InArgument<string> CertificateThumbprint { get; set; }

        [RequiredArgument]
        public InArgument<string> DeployName { get; set; }

        [RequiredArgument]
        public InArgument<string> DeployLabel { get; set; }

        [RequiredArgument]
        public InArgument<string> StorageAccountName { get; set; }

        [RequiredArgument]
        public InArgument<string> ConfigurationFilepath { get; set; }

        [RequiredArgument]
        public InArgument<string> PackageFilepath { get; set; }

        [RequiredArgument]
        public InArgument<string> BlobContainer { get; set; }

        public InArgument<string> BlobStorageFolder { get; set; }

        [RequiredArgument]
        public InArgument<bool> StartAfterDeploy { get; set; }

        [RequiredArgument]
        public InArgument<bool> WaitForDeployCompletion { get; set; }

        [RequiredArgument]
        public InArgument<string> DeploymentWorkflow { get; set; }

        #endregion

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            var subscriptionId = SubscriptionId.Get(context);
            var serviceName = ServiceName.Get(context);
            var certificateThumbprint = CertificateThumbprint.Get(context);
            var deployName = DeployName.Get(context);
            var deployLabel = DeployLabel.Get(context);
            var storageAccountName = StorageAccountName.Get(context);
            var configurationFilepath = ConfigurationFilepath.Get(context);
            var packageFilepath = PackageFilepath.Get(context);
            var blobContainer = BlobContainer.Get(context);
            var blobStorageFolder = BlobStorageFolder.Get(context);
            var startAfterDeploy = StartAfterDeploy.Get(context);
            var deploymentWorkflow = DeploymentWorkflow.Get(context);
            var waitForDeployCompletion = WaitForDeployCompletion.Get(context);

            #region Validate Parameters
// ReSharper disable NotResolvedInText
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                throw new ArgumentNullException("SubscriptionId");
            }

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentNullException("ServiceName");
            }

            if (string.IsNullOrWhiteSpace(certificateThumbprint))
            {
                throw new ArgumentNullException("CertificateThumbprint");
            }

            if (string.IsNullOrWhiteSpace(deployName))
            {
                throw new ArgumentNullException("DeployName");
            }

            if (string.IsNullOrWhiteSpace(deployLabel))
            {
                throw new ArgumentNullException("DeployLabel");
            }

            if (string.IsNullOrWhiteSpace(storageAccountName))
            {
                throw new ArgumentNullException("StorageAccountName");
            }

            if (string.IsNullOrWhiteSpace(configurationFilepath))
            {
                throw new ArgumentNullException("ConfigurationFilepath");
            }

            if (string.IsNullOrWhiteSpace(packageFilepath))
            {
                throw new ArgumentNullException("PackageFilepath");
            }

            if (string.IsNullOrWhiteSpace(blobContainer))
            {
                throw new ArgumentNullException("BlobContainer");
            }

            if (string.IsNullOrWhiteSpace(blobStorageFolder))
            {
                throw new ArgumentNullException("BlobStorageFolder");
            }

            if (string.IsNullOrWhiteSpace(deploymentWorkflow))
            {
                throw new ArgumentNullException("DeploymentWorkflow");
            }
// ReSharper restore NotResolvedInText

            #endregion

            // Make sure certain values are set to lowercase as Azure requires
            storageAccountName = storageAccountName.ToLower();
            blobContainer = blobContainer.ToLower();
            
            try
            {
                var workflowDecoder = new WorkflowDecoder();

                // Attempt to extract the workflow name.  It may be misspelled so decode if you can
                var decodedDeploymentWorkflow = workflowDecoder.Execute(deploymentWorkflow);

                if (decodedDeploymentWorkflow == null)
                {
                    throw new ArgumentException(
                        string.Format("DeploymentWorkflow value of {0} could not be decoded into any of the valid values: ('StagingNoDelete', 'StagingDelete', 'ProductionNoDelete', 'ProductionDelete', 'ProductionDemoteNoDelete', ProductionDemoteDelete')", 
                        deploymentWorkflow));
                }

                var workflow = (DeploymentWorkflowTargets) Enum.Parse(typeof(DeploymentWorkflowTargets), decodedDeploymentWorkflow);

                IDeployOperation deployOperation = new DeployOperation();

                new DeployOperation().Execute(subscriptionId, serviceName, certificateThumbprint, deployName,
                                        deployLabel, storageAccountName, configurationFilepath, packageFilepath,
                                        blobContainer,
                                        blobStorageFolder, startAfterDeploy, waitForDeployCompletion, context, workflow);
            }
            catch(Exception exception)
            {
                // Write to the log
                context.WriteBuildMessage(string.Format("{0} - {1}", exception.Message, exception.InnerException), BuildMessageImportance.High);

                throw;
            }
        }
    }
}
