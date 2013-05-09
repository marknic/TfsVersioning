// ==============================================================================================
// AzureLM - Azure Lifecycle Management Extension for Team Foundation Server
//
// Author: Mark S. Nichols
//
// Copyright (c) 2013 Microsoft Corporation
//
// ==============================================================================================

namespace TfsBuild.AzureLm.Activities.Enumerations
{
    public enum DeploymentWorkflowTargets
    {
        None = 0,                       // No deployment steps are to be taken
        ProductionNoDelete = 1,         // Deploy only if there is no existing deployment in the production slot (No VIP Swap)
        ProductionDelete = 2,           // If a deployment exists in the production slot, delete it and then continue with deploying to production (No VIP Swap)
        StagingNoDelete = 3,            // Deploy only if there is no existing deployment in the staging slot (No VIP Swap)
        StagingDelete = 4,              // If a deployment exists in the staging slot, delete it and then continue with deploying to staging (No VIP Swap)
        ProductionDemoteNoDelete = 5,   // Use the "Demotion" protocol: If a production deployment exists and if no existing deployment exists in staging, deploy to staging and then VIP Swap.  If production does not exist, deploy directly to production with no VIP Swap.  If staging and production exist, no deployment takes place.
        ProductionDemoteDelete = 6,     // Use the "Demotion" protocol: If a production deployment exists, delete any existing deployment in staging, deploy to staging and then VIP Swap.  If production does not exist, deploy directly to production with no VIP Swap.
        UploadOnly = 7
    }
}


