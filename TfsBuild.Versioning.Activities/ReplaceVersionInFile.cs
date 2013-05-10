using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Activities;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Build.Client;

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
    [ToolboxBitmap(typeof(ReplaceVersionInFile), "Resources.version.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class ReplaceVersionInFile : CodeActivity
    {
        #region Workflow Arguments

        [RequiredArgument]
        public InArgument<VersionTypeOptions> VersionType { get; set; }
        
        [RequiredArgument]
        public InArgument<string> ReplacementVersion { get; set; }

        [RequiredArgument]
        public InArgument<string> FilePath { get; set; }

        [RequiredArgument]
        public InArgument<bool> ForceCreate { get; set; }

        #endregion

        /// <summary>
        /// Searches a file for a the version number and replaces with the updated value 
        /// </summary>
        /// <param name="context"></param>
        protected override void Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            var versionType = context.GetValue(VersionType);
            var replacementVersion = context.GetValue(ReplacementVersion);
            var filePath = context.GetValue(FilePath);
            var forceCreate = context.GetValue(ForceCreate);
            var newFileData = new StringBuilder();

            #region Validate Arguments

            if (String.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(
                    "You must provide an AssemblyInfo file path", "FilePath");
            }

            if (String.IsNullOrEmpty(replacementVersion))
            {
                throw new ArgumentException(
                     "You must provide a new version to insert", "ReplacementVersion");
            }

            #endregion

            var versionName = Enum.GetName(typeof(VersionTypeOptions), versionType);

            context.WriteBuildMessage(string.Format("Replacing {0} in {1} with {2}", versionName,
                                            Path.GetFileName(filePath), replacementVersion), BuildMessageImportance.High);
            
            // generate the regex value that will be used to do the search and the replacement
            var versionRegex =
                string.Format(@"(?<AssemblyVersionLine>(?<AssemblyPrefix>[\[<].*{0}.*\(\x22)(?<VersionNo>.*)(?<AssemblyPostfix>\x22\)[\]>]+))", versionName);

            var regex = new Regex(versionRegex);

            // make sure you can write to the file
            var currentFileAttributes = File.GetAttributes(filePath);
            File.SetAttributes(filePath, currentFileAttributes & ~FileAttributes.ReadOnly);

            // Get the file data
            var fileData = File.ReadAllText(filePath);
            
            // perform the actual replacement
            var groups = regex.Match(fileData).Groups;
            var normalizedFileExt = Path.GetExtension(filePath).ToLower();

            // Assembly*Version not found so insert it if forceCreate is true
            if ((groups.Count < 5) && (forceCreate))
            {
                newFileData.Append(InsertAssemblyVersion(normalizedFileExt, versionName, fileData, replacementVersion));
            }
            else
            {
                // Assembly*Version found...replace the value
                var replacementString = string.Format("{0}{1}{2}", groups["AssemblyPrefix"], replacementVersion, groups["AssemblyPostfix"]);

                newFileData.Append(regex.Replace(fileData, replacementString));
            }

            // Write the data out to a file
            File.WriteAllText(filePath, newFileData.ToString());

            // restore the file's original attributes
            File.SetAttributes(filePath, currentFileAttributes);
        }


        /// <summary>
        /// Inserts the updated version into the file
        /// 
        /// Note: Thanks to Rickasaurus for his suggestion and his code to version F# assemblies
        /// 
        /// </summary>
        /// <param name="fileExtension">extension of the file being modified (the language)</param>
        /// <param name="versionTitle">the version attribute being changed</param>
        /// <param name="fileData">the current file being modified</param>
        /// <param name="version">the new version to insert</param>
        /// <returns>the updated file data</returns>
        public static string InsertAssemblyVersion(string fileExtension, string versionTitle, string fileData, string version)
        {
            var fileDataOut = new StringBuilder();
            var versionShell = "[assembly: {0}(\"{1}\")]";
            var normalizedFileExt = fileExtension.ToLower();

            // if working with F# files, remove the "do binding" so we can make sure that the "do" is at the end of the file
            if (normalizedFileExt == ".fs")
            {
                var regex = new Regex(@".*(\(\s*\)|do\s*\(\s*\))");
                fileData = regex.Replace(fileData, "");
            }
 
            fileDataOut.Append(fileData);

            // default is C#, change if different)
            switch (normalizedFileExt)
            {
                case ".cpp":
                    versionShell = "[assembly: {0}Attribute(\"{1}\")];";
                    break;

                case ".vb":
                    versionShell = "<Assembly: {0}(\"{1}\")>";
                    break;

                case ".fs":
                    versionShell = "[<assembly: {0}(\"{1}\")>]";
                    break;
            }

            var versionLine = string.Format(versionShell, versionTitle, version);

            fileDataOut.AppendLine();
            fileDataOut.Append(versionLine);
            fileDataOut.AppendLine();

            // for F#, put the do() binding back in
            if (normalizedFileExt == ".fs")
            {
                fileDataOut.Append("do ()");
                fileDataOut.AppendLine();
            }

            return fileDataOut.ToString();
        }
    }
}
