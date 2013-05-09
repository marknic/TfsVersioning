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
    public enum DeploymentStatus
    {
        Running,
        Suspended,
        RunningTransitioning,
        SuspendedTransitioning,
        Starting,
        Suspending,
        Deploying,
        Deleting
    }
}
