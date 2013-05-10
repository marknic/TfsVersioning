using System.Activities;
using System.Drawing;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

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
    [ToolboxBitmap(typeof(CheckInFiles), "Resources.version.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class CheckInFiles : CodeActivity
    {
        #region Workflow Arguments

        /// <summary>
        /// The project workspace
        /// </summary>
        [RequiredArgument]
        public InArgument<Workspace> Workspace { get; set; }

        #endregion
        
        /// <summary>
        /// Check in the files that were checked out and edited
        /// </summary>
        /// <param name="context"></param>
        protected override void Execute(CodeActivityContext context)
        {
            // Get the workspace passed in as a property
            var workspace = context.GetValue(Workspace);

            // Get the list of changes that need to be made.
            PendingChange[] pendingChanges = workspace.GetPendingChanges();

            if ((pendingChanges == null) || (pendingChanges.Length == 0))
            {
                context.WriteBuildMessage("There are no changes pending - No CheckIn will be performed.", BuildMessageImportance.High);
            }
            else
            {
                context.WriteBuildMessage("Checking in pending changes", BuildMessageImportance.High);

                // Need to check in the files without initiating a CI build: Passing on "***NO_CI***" as a comment does this
                //  see: http://blogs.msdn.com/b/buckh/archive/2007/07/27/tfs-2008-how-to-check-in-without-triggering-a-build-when-using-continuous-integration.aspx
                workspace.CheckIn(workspace.GetPendingChanges(), "Versioning Build Process", "***NO_CI***", null, null,
                                  new PolicyOverrideInfo(
                                      "Checking in modified AssemblyInfo files as part of Versioning Build Process",
                                      null), CheckinOptions.SuppressEvent);
            }
        }
    }
}
