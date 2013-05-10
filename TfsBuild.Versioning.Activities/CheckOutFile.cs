using System.Drawing;
using System.Activities;
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
    [ToolboxBitmap(typeof(CheckOutFile), "Resources.version.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class CheckOutFile : CodeActivity
    {
        #region Workflow Arguments

        /// <summary>
        /// This is the workspace used by the project being built
        /// </summary>
        [RequiredArgument]
        public InArgument<Workspace> Workspace { get; set; }

        /// <summary>
        /// The path to the file to be checked out
        /// </summary>
        [RequiredArgument]
        public InArgument<string> FileToCheckOut { get; set; }

        #endregion

        /// <summary>
        /// Check a file out for edit
        /// </summary>
        /// <param name="context"></param>
        protected override void Execute(CodeActivityContext context)
        {
            // xlate the values into usable types
            var fileToCheckOut = context.GetValue(FileToCheckOut);
            var workspace = context.GetValue(Workspace);

            context.WriteBuildMessage(string.Format("Checking out file for modification: {0}", fileToCheckOut), BuildMessageImportance.High);

            // Check the file out for edit
            workspace.PendEdit(fileToCheckOut);
        }
    }
}
