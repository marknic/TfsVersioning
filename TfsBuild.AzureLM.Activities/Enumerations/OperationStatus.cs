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
    /// <summary>
    /// The operation status values from PollGetOperationStatus.
    /// </summary>
    public enum AzureApiOperationStatus
    {
        InProgress,
        Failed,
        Succeeded,
        TimedOut
    }
}
