using System;
using System.Net;
using TfsBuild.AzureLm.Activities.Enumerations;

// ==============================================================================================
// AzureLM - Azure Lifecycle Management Extension for Team Foundation Server
//
// Author: Mark S. Nichols
//
// Copyright (c) 2013 Microsoft Corporation
//
// ==============================================================================================

namespace TfsBuild.AzureLm.Activities.Models
{
    /// <summary>
    /// The results from PollGetOperationStatus are passed in this struct.
    /// </summary>
    public struct PollingResult
    {
        // The status: InProgress, Failed, Succeeded, or TimedOut.
        public AzureApiOperationStatus Status { get; set; }

        // The http status code of the requestId operation, if any.
        public HttpStatusCode StatusCode { get; set; }

        // The approximate running time for PollGetOperationStatus.
        public TimeSpan RunningTime { get; set; }

        // The error code for the failed operation.
        public string Code { get; set; }

        // The message for the failed operation.
        public string Message { get; set; }
    }
}
