using System;
using System.Drawing;
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
    [ToolboxBitmap(typeof(ConvertAssemblyPropertyPattern), "Resources.version.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class ConvertAssemblyPropertyPattern : CodeActivity
    {
        #region Workflow Arguments

        /// <summary>
        /// The pattern to convert
        /// </summary>
        [RequiredArgument]
        public InArgument<string> PropertyPattern { get; set; }

        /// <summary>
        /// Build number prefix for differentiating the build versions
        /// </summary>
        [RequiredArgument]
        public InArgument<int> BuildNumberPrefix { get; set; }

        /// <summary>
        /// The date of the build
        /// </summary>
        [RequiredArgument]
        public InArgument<DateTime> BuildDate { get; set; }

        [RequiredArgument]
        public InArgument<IBuildDetail> BuildDetail { get; set; }

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
            string propertyPattern = context.GetValue(PropertyPattern);
            IBuildDetail buildDetail = context.GetValue(BuildDetail);
            DateTime buildDate = context.GetValue(BuildDate);
            int buildNumberPrefix = context.GetValue(BuildNumberPrefix);

            // Validate the version pattern
            if (string.IsNullOrEmpty(propertyPattern))
            {
                throw new ArgumentException("PropertyPattern must contain a valid property replacement pattern.");
            }

            // Validate the version pattern
            if (buildDetail == null)
            {
                throw new ArgumentNullException("BuildDetail", "BuildDetail must contain a valid IBuildDetail value.");
            }

            string convertedValue = VersioningHelper.ReplacePatternWithValue(propertyPattern, buildDetail, buildDetail.BuildNumber, buildNumberPrefix, buildDate);

            // Return the value back to the workflow
            context.SetValue(ConvertedVersionNumber, convertedValue);
        }
    }
}
