using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
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
    [ToolboxBitmap(typeof(ReplaceAssemblyInfoProperties), "Resources.version.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class ReplaceAssemblyInfoProperties : CodeActivity
    {
        #region Workflow Arguments

        [RequiredArgument]
        public InArgument<string> FilePath { get; set; }

        [RequiredArgument]
        public InArgument<bool> ForceCreate { get; set; }

        /// <summary>
        /// The date of the build
        /// </summary>
        [RequiredArgument]
        public InArgument<DateTime> BuildDate { get; set; }

        [RequiredArgument]
        public InArgument<IBuildDetail> BuildDetail { get; set; }

        // Assembly properties
        public InArgument<string> AssemblyTitle { get; set; }
        public InArgument<string> AssemblyDescription { get; set; }
        public InArgument<string> AssemblyConfiguration { get; set; }
        public InArgument<string> AssemblyCompany { get; set; }
        public InArgument<string> AssemblyProduct { get; set; }
        public InArgument<string> AssemblyCopyright { get; set; }
        public InArgument<string> AssemblyTrademark { get; set; }
        public InArgument<string> AssemblyCulture { get; set; }
        public InArgument<string> AssemblyInformationalVersion { get; set; }

        #endregion

        /// <summary>
        /// Searches a file for a the version number and replaces with the updated value 
        /// </summary>
        /// <param name="context"></param>
        protected override void Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            var filePath = context.GetValue(FilePath);
            var forceCreate = context.GetValue(ForceCreate);
            IBuildDetail buildDetail = context.GetValue(BuildDetail);
            DateTime buildDate = context.GetValue(BuildDate);
    
            #region Validate Arguments

            if (String.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(
                    "You must provide a file path", "FilePath");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found:", filePath);
            }

            #endregion

            IList<KeyValuePair<string, string>> inputValues = new ReadOnlyCollectionBuilder<KeyValuePair<string, string>>();

            inputValues.Add(new KeyValuePair<string,string>( "AssemblyTitle", context.GetValue(AssemblyTitle)));
            inputValues.Add(new KeyValuePair<string,string>( "AssemblyDescription", context.GetValue(AssemblyDescription)));
            inputValues.Add(new KeyValuePair<string,string>( "AssemblyConfiguration", context.GetValue(AssemblyConfiguration)));
            inputValues.Add(new KeyValuePair<string,string>( "AssemblyCompany", context.GetValue(AssemblyCompany)));
            inputValues.Add(new KeyValuePair<string,string>( "AssemblyProduct", context.GetValue(AssemblyProduct)));
            inputValues.Add(new KeyValuePair<string,string>( "AssemblyCopyright", context.GetValue(AssemblyCopyright)));
            inputValues.Add(new KeyValuePair<string,string>( "AssemblyTrademark", context.GetValue(AssemblyTrademark)));
            inputValues.Add(new KeyValuePair<string,string>( "AssemblyCulture", context.GetValue(AssemblyCulture)));
            inputValues.Add(new KeyValuePair<string,string>( "AssemblyInformationalVersion", context.GetValue(AssemblyInformationalVersion)));
            
            // Get the arguments and values from the current context
            IList<KeyValuePair<string, string>> assemblyInfoProperties = VersioningHelper.GetArgumentValues(inputValues);

            // What type of project are we working on?
            var projectType = VersioningHelper.GetProjectTypeFromFileName(filePath);

            // Perform the update of the assembly info values based on the list created above
            UpdateAssemblyValues(filePath, assemblyInfoProperties, buildDetail, buildDate, projectType, forceCreate);
        }

        /// <summary>
        /// Walks through the list of provided AssemblyInfo properties and updates those values 
        /// </summary>
        /// <param name="filePath">AssemblyInfo file being modified</param>
        /// <param name="assemblyInfoProperties">List of properties and values to change</param>
        /// <param name="buildDetail"></param>
        /// <param name="buildDate"></param>
        /// <param name="projectType">Type of project (cs, vb, cpp or fs)</param>
        /// <param name="forceCreate">If the value isn't in the AssemblyInfo file do we insert it anyway</param>
        public void UpdateAssemblyValues(string filePath, IList<KeyValuePair<string, string>> assemblyInfoProperties, 
            IBuildDetail buildDetail, DateTime buildDate, ProjectTypes projectType, bool forceCreate)
        {
            var newFileData = new StringBuilder();

            // make sure you can write to the file
            var currentFileAttributes = File.GetAttributes(filePath);
            File.SetAttributes(filePath, currentFileAttributes & ~FileAttributes.ReadOnly);

            // Get the file data
            var fileData = File.ReadAllText(filePath);

            // if working with F# files, remove the "do binding" so we can make sure that the "do" is at the end of the file
            if (projectType == ProjectTypes.Fs)
            {
                var regex = new Regex(@".*(\(\s*\)|do\s*\(\s*\))"); 
                fileData = regex.Replace(fileData, "");
            }

            foreach (KeyValuePair<string, string> property in assemblyInfoProperties)
            {
                string convertedValue = VersioningHelper.ReplacePatternsInPropertyValue(property.Value, buildDetail, 0,
                                                                                        buildDate);

                fileData = UpdateAssemblyValue(fileData, property.Key, convertedValue, projectType, forceCreate);
            }

            // do we need to put a NewLine char in the data from the file
            if (DoesLastLineContainCr(fileData))
            {
                newFileData.Append(fileData);
            }
            else
            {
                newFileData.AppendLine(fileData);
            }

            // for F#, put the do() binding back in
            if (projectType == ProjectTypes.Fs)
            {
                newFileData.AppendLine("do ()");
            }

            // Write the data out to a file
            File.WriteAllText(filePath, newFileData.ToString());

            // restore the file's original attributes
            File.SetAttributes(filePath, currentFileAttributes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileData">AssemblyInfo file contents as a string</param>
        /// <param name="propertyName">Name of the property to modify or insert</param>
        /// <param name="propertyValue">New value of the property</param>
        /// <param name="projectType">Type of project (cs, vb, cpp or fs)</param>
        /// <param name="forceCreate">If the value isn't in the AssemblyInfo file do we insert it anyway</param>
        /// <returns>The updated file data</returns>
        public static string UpdateAssemblyValue(string fileData, string propertyName, string propertyValue, ProjectTypes projectType, bool forceCreate)
        {
            string newFileData;
            
            // Set up RegEx
            var regExExpression = string.Format(VersioningHelper.RegExPropertyMatch, propertyName);
            var regex = new Regex(regExExpression);

            // perform the actual replacement
            var groups = regex.Match(fileData).Groups;

            // AssemblyInfo Property not found so insert it if forceCreate is true
            // 5 is the number of groups that will be discovered in the regex expression match
            if ((groups.Count < 5) && (forceCreate))
            {
                newFileData = VersioningHelper.InsertAssemblyInfoProperty(fileData, propertyName, propertyValue, projectType);
            }
            else
            {
                // Property was found...replace the value
                var replacementString = string.Format("{0}{1}{2}", groups["PropertyPrefix"], propertyValue, groups["PropertyPostfix"]);

                newFileData = regex.Replace(fileData, replacementString);
            }

            return newFileData;
        }

        public static bool DoesLastLineContainCr(string fileData)
        {
            var regEx = new Regex(@".+\n$");

            var match = regEx.Match(fileData);

            if (match.Success)
            {
                return true;
            }

            return false;
        }
    }
}

