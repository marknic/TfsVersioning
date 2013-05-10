using System;
using System.Drawing;
using System.Text;
using System.Activities;
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
    /// <summary>
    /// Takes in a version pattern and turns it into a version number.
    /// </summary>
    [ToolboxBitmap(typeof(ConvertVersionPattern), "Resources.version.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class ConvertVersionPattern : CodeActivity
    {
        #region Workflow Arguments

        /// <summary>
        /// The pattern to convert
        /// </summary>
        [RequiredArgument]
        public InArgument<string> VersionPattern { get; set; }
         
        /// <summary>
        /// TFS build number in case the "B" pattern is used
        /// </summary>
        [RequiredArgument]
        public InArgument<string> BuildNumber { get; set; }

        /// <summary>
        /// The prefix value to add to the build number to make it unique compared to other builds
        /// </summary>
        [RequiredArgument]
        public InArgument<int> BuildNumberPrefix { get; set; }

        /// <summary>
        /// The converted version number 
        /// </summary>
        public OutArgument<string> ConvertedVersionNumber { get; set; }

        #endregion

        /// <summary>
        /// Processes the conversion of the version number
        /// </summary>
        /// <param name="context"></param>
        protected override void Execute(CodeActivityContext context)
        {
            // Get the values passed in
            var versionPattern = context.GetValue(VersionPattern);
            var buildNumber = context.GetValue(BuildNumber);
            var buildNumberPrefix = context.GetValue(BuildNumberPrefix);
 
            var version = new StringBuilder();
            var addDot = false;
           
            // Validate the version pattern
            if (string.IsNullOrEmpty(versionPattern))
            {
                throw new ArgumentException("VersionPattern must contain the versioning pattern.");
            }

            var versionPatternArray = versionPattern.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);

            // Go through each pattern and convert it
            foreach (var conversionItem in versionPatternArray)
            {
                if (addDot) { version.Append("."); }

                version.Append(VersioningHelper.ReplacePatternWithValue(conversionItem, buildNumber, buildNumberPrefix, DateTime.Now));

                addDot = true;
            }

            // Return the value back to the workflow
            context.SetValue(ConvertedVersionNumber, version.ToString());
        }
    }
}
