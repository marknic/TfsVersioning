using System;
using System.Text;
using TfsBuild.AzureLM.Activities.Models;
using TfsBuild.AzureLm.Activities.Enumerations;

// ==============================================================================================
// AzureLM - Azure Lifecycle Management Extension for Team Foundation Server
//
// Author: Mark S. Nichols
//
// Copyright (c) 2013 Microsoft Corporation
//
// ==============================================================================================

namespace TfsBuild.AzureLm.Activities.Utility
{
    public static class DeploymentSupport
    {
        /// <summary>
        /// Helper method to encode a string into Base64
        /// </summary>
        /// <param name="dataIn">String to encode</param>
        /// <returns>Base64 encoded string</returns>
        public static string EncodeToBase64String(string dataIn)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(dataIn));
        }


        /// <summary>
        /// Converts a Base-64 encoded string to UTF-8.
        /// </summary>
        /// <param name="encodedString">The string to convert from Base-64.</param>
        /// <returns>The converted UTF-8 string.</returns>
        public static string DecodeFromBase64String(string encodedString)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedString));
        }


        /// <summary>
        /// Based on the workflow indicated and the existance of staging and/or production, this method
        /// calculates the destination for the deployment, if there is a deployment that must be deleted
        /// first and if VIP Swap is necessary
        /// </summary>
        /// <param name="deploymentWorkflow"></param>
        /// <param name="stagingExists"></param>
        /// <param name="productionExists"></param>
        /// <returns></returns>
        public static DeploymentWorkflowIndicators DecodeWorkflowData(DeploymentWorkflowTargets deploymentWorkflow, bool stagingExists, bool productionExists)
        {
            var deploymentSlot = DeploymentSlots.None;
            var doDelete = false;
            var doSwap = false;

            var deploymentInWay = ((deploymentWorkflow == DeploymentWorkflowTargets.StagingNoDelete) && stagingExists) ||
                                    ((deploymentWorkflow == DeploymentWorkflowTargets.ProductionNoDelete) && productionExists) ||
                                    ((deploymentWorkflow == DeploymentWorkflowTargets.ProductionDemoteNoDelete) &&
                                     productionExists && stagingExists);

            switch (deploymentWorkflow)
            {
                case DeploymentWorkflowTargets.StagingNoDelete:
                    if (!deploymentInWay)
                    {
                        deploymentSlot = DeploymentSlots.Staging;
                    }
                    break;

                case DeploymentWorkflowTargets.StagingDelete:
                    deploymentSlot = DeploymentSlots.Staging;
                    doDelete = stagingExists;
                    break;

                case DeploymentWorkflowTargets.ProductionNoDelete:
                    if (!deploymentInWay)
                    {
                        deploymentSlot = DeploymentSlots.Production;
                    }
                    break;

                case DeploymentWorkflowTargets.ProductionDelete:
                    deploymentSlot = DeploymentSlots.Production;
                    doDelete = productionExists;
                    break;

                case DeploymentWorkflowTargets.ProductionDemoteNoDelete:
                    if (!deploymentInWay)
                    {
                        deploymentSlot = productionExists ? DeploymentSlots.Staging : DeploymentSlots.Production;
                    }
                    doSwap = deploymentSlot == DeploymentSlots.Staging;
                    break;

                case DeploymentWorkflowTargets.ProductionDemoteDelete:
                    deploymentSlot = productionExists ? DeploymentSlots.Staging : DeploymentSlots.Production;
                    doDelete = ((deploymentSlot == DeploymentSlots.Production) && productionExists) ||
                               ((deploymentSlot == DeploymentSlots.Staging) && stagingExists);
                    doSwap = deploymentSlot == DeploymentSlots.Staging;
                    break;

                case DeploymentWorkflowTargets.UploadOnly:
                    deploymentSlot = DeploymentSlots.None;
                    break;
            }

            var deploymentWorkflowIndicators = new DeploymentWorkflowIndicators
                {
                    DeploymentSlot = deploymentSlot,
                    DeploymentInWay = deploymentInWay,
                    DoDelete = doDelete,
                    DoSwap = doSwap
                };

            return deploymentWorkflowIndicators;
        }
    }
}
