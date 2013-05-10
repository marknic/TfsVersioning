using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Tracking;
using Microsoft.TeamFoundation.Build.Workflow.Activities;

// ==============================================================================================
// http://tfsversioning.codeplex.com/
//
// Author: Mark S. Nichols
//
// Copyright (c) 2011 Microsoft Corporation
//
// This source is subject to the Microsoft Permissive License. 
// ==============================================================================================

namespace TfsBuild.Versioning.Activities
{
    internal static class BuildContextExtensions
    {
        /// <summary>
        /// Extension method to help writing messages to the build context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="messageImportance"></param>
        public static void WriteBuildMessage(this CodeActivityContext context, string message, BuildMessageImportance messageImportance)
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
