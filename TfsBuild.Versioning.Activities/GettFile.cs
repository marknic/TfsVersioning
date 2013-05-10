using System.Drawing;
using System.Activities;
using System.IO;
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
    [ToolboxBitmap(typeof(GetFile), "Resources.version.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class GetFile : CodeActivity
    {
        /// <summary>
        /// This is the workspace used by the project being built
        /// </summary>
        [RequiredArgument]
        public InArgument<Workspace> Workspace { get; set; }

        /// <summary>
        /// The path to the file to be checked out
        /// </summary>
        [RequiredArgument]
        public InArgument<string> FileToGet { get; set; }

        /// <summary>
        /// The path to the local build folder
        /// </summary>
        [RequiredArgument]
        public InArgument<string> BuildDirectory { get; set; }

        /// <summary>
        /// Full path to the version seed file
        /// </summary>
        public OutArgument<string> FullPathToSeedFile { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            #region Workflow Arguments

            // The TFS source location of the file to get
            var fileToGet = context.GetValue(FileToGet);

            // The current workspace - used to create a new workspace for the get
            var workspace = context.GetValue(Workspace);

            // The local build directory
            var buildDirectory = context.GetValue(BuildDirectory);

            #endregion

            // Version seed file and path
            var versionFileDirectory = string.Format("{0}\\VersionSeed", buildDirectory);
            var filename = Path.GetFileName(fileToGet);
            var fullPathToSeedFile = Path.Combine(versionFileDirectory, filename);

            // Write to the log
            context.WriteBuildMessage(string.Format("Getting file from Source: {0}", fileToGet), BuildMessageImportance.High);
            context.WriteBuildMessage(string.Format("Placing version seed file in: {0}", versionFileDirectory), BuildMessageImportance.High);

            // Create workspace and working folder
            var tempWorkspace = workspace.VersionControlServer.CreateWorkspace("VersionSeed");
            var workingFolder = new WorkingFolder(fileToGet, fullPathToSeedFile);

            // Map the workspace
            tempWorkspace.CreateMapping(workingFolder);

            // Get the file
            var request = new GetRequest(new ItemSpec(fileToGet, RecursionType.None), VersionSpec.Latest);
            var status = tempWorkspace.Get(request, GetOptions.GetAll | GetOptions.Overwrite); 

            if (!status.NoActionNeeded)
            {
                foreach (var failure in status.GetFailures())
                {
                    context.WriteBuildMessage(string.Format("Failed to get file from source: {0} - {1}", fileToGet, failure.GetFormattedMessage()), BuildMessageImportance.High);
                }    
            }

            // Return the value back to the workflow
            context.SetValue(FullPathToSeedFile, fullPathToSeedFile);

            // Get rid of the workspace
            tempWorkspace.Delete();
        }
    }
}
