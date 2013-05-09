using System;
using System.Drawing;
using System.Activities;
using System.IO;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using TfsBuild.AzureLm.Activities.Utility;

// ==============================================================================================
// Author: Mark S. Nichols
//
// Copyright (c) 2013 Microsoft Corporation
//
// This source is subject to the Microsoft Permissive License. 
// ==============================================================================================

namespace TfsBuild.AzureLm.Activities
{
    [ToolboxBitmap(typeof(GetFileFromSourceControl), "Resources.version.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class GetFileFromSourceControl : CodeActivity
    {
        private const string TempWorkspaceName = "GetFileWS88926";

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
        /// The path to the destination folder
        /// </summary>
        [RequiredArgument]
        public InArgument<string> DestinationFolder { get; set; }

        /// <summary>
        /// Full path to the Retrieved file
        /// </summary>
        public OutArgument<string> FullPathToRetrievedFile { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            #region Workflow Arguments

            // The TFS source location of the file to get
            var fileToGet = context.GetValue(FileToGet);

            // The current workspace - used to create a new workspace for the get
            var workspace = context.GetValue(Workspace);

            // The local build directory
            var destinationFolder = context.GetValue(DestinationFolder);

            #endregion

            // filename and path
            var filename = Path.GetFileName(fileToGet);

            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("File To Get did not contain a filename");
            }

            var fullPathToRetrievedFile = Path.Combine(destinationFolder, filename);

            // Write to the log
            context.WriteBuildMessage(string.Format("Getting file from Source: {0}", fileToGet), BuildMessageImportance.High);
            context.WriteBuildMessage(string.Format("Placing retrieved file in: {0}", destinationFolder), BuildMessageImportance.High);

            context.WriteBuildMessage(string.Format("Workspace OwnerName: {0}", workspace.OwnerName), BuildMessageImportance.High);

            Workspace tmpWorkspace;

            try
            {
                context.WriteBuildMessage(string.Format("Getting temp workspace: {0}", TempWorkspaceName), BuildMessageImportance.High);
                tmpWorkspace = workspace.VersionControlServer.GetWorkspace(TempWorkspaceName, workspace.VersionControlServer.AuthorizedUser);
            }
            catch (WorkspaceNotFoundException)
            {
                context.WriteBuildMessage(string.Format("Get workspace failed: Creating temp workspace: {0}", TempWorkspaceName), BuildMessageImportance.High);
                tmpWorkspace = workspace.VersionControlServer.CreateWorkspace(TempWorkspaceName, workspace.VersionControlServer.AuthorizedUser);
            }

            context.WriteBuildMessage(string.Format("existingWorkspace was null: {0}", tmpWorkspace == null), BuildMessageImportance.High);

            if (tmpWorkspace == null)
            {
                throw new ApplicationException("Temporary workspace could not be found or created.");
            }

            context.WriteBuildMessage(string.Format("Mapping from: {0} to {1}", Path.GetDirectoryName(fileToGet), Path.GetDirectoryName(fullPathToRetrievedFile)), BuildMessageImportance.High);
            var workingFolder = new WorkingFolder(Path.GetDirectoryName(fileToGet), Path.GetDirectoryName(fullPathToRetrievedFile));

            // Map the workspace
            context.WriteBuildMessage("Calling CreateMapping:", BuildMessageImportance.High);
            tmpWorkspace.CreateMapping(workingFolder);

            // Get the file
            context.WriteBuildMessage("Creating GetRequest:", BuildMessageImportance.High);
            var request = new GetRequest(new ItemSpec(fileToGet, RecursionType.None), VersionSpec.Latest);

            context.WriteBuildMessage("Calling tempWorkspace.Get:", BuildMessageImportance.High);
            var status = tmpWorkspace.Get(request, GetOptions.GetAll | GetOptions.Overwrite); 

            if (!status.NoActionNeeded)
            {
                foreach (var failure in status.GetFailures())
                {
                    context.WriteBuildMessage(string.Format("Failed to get file from source: {0} - {1}", 
                        fileToGet, failure.GetFormattedMessage()), BuildMessageImportance.High);
                }    
            }

            // Return the value back to the workflow
            context.SetValue(FullPathToRetrievedFile, fullPathToRetrievedFile);

            // Get rid of the workspace
            context.WriteBuildMessage("Deleting the tempWorkspace:", BuildMessageImportance.High);
            tmpWorkspace.Delete();
        }
    }
}
