using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Tracking;

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

    internal static class BuildContextExtensions
    {
        /// <summary>
        /// Extension method to help writing messages to the build context
        /// </summary>
        /// <param name="context">this object</param>
        /// <param name="message">The message to send to the build log</param>
        /// <param name="messageImportance">"Importance" or which level of logging detail will the message appear</param>
        public static void WriteBuildMessage(this CodeActivityContext context, string message, BuildMessageImportance messageImportance)
        {
            if (context != null)
            {
                context.Track(new BuildInformationRecord<BuildMessage>
                    {
                        Value = new BuildMessage
                            {
                                Importance = messageImportance,
                                Message = message,
                            },
                    });
            }
        }
    }
}