using System;
using System.Activities;
using System.Drawing;
using System.IO;
using System.Xml;
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
    /// Used to search the version "seed file" and return the value
    /// </summary>
    [ToolboxBitmap(typeof(XmlExtract), "Resources.version.ico")]
    [BuildActivity(HostEnvironmentOption.All)]
    [BuildExtension(HostEnvironmentOption.All)]
    public sealed class XmlExtract : CodeActivity
    {
        #region Workflow Arguments

        /// <summary>
        /// Specify the path of the file to replace occurences of the regular 
        /// expression with the replacement text
        /// </summary>
        [RequiredArgument]
        public InArgument<string> FilePath { get; set; }

        /// <summary>
        /// Regular expression to search for and replace in the specified
        /// text file.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> XPathExpression { get; set; }

        /// <summary>
        /// The value of the node found via the XPath Expression
        /// </summary>
        public OutArgument<string> NodeValue { get; set; }

        #endregion

        /// <summary>
        /// Searches an XML file with an XPath expression
        /// </summary>
        /// <param name="context"></param>
        protected override void Execute(CodeActivityContext context)
        {
            // get the value of the XPathExpression
            var xpathExpression = context.GetValue(XPathExpression);

            // get the value of the FilePath
            var filePath = context.GetValue(FilePath);

            // validate that there is a filename to work with
            if (String.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("FilePath");
            }

            // validate that there is a file
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            // Validate that there is an XPath expression for the search
            if (String.IsNullOrEmpty(xpathExpression))
            {
                throw new ArgumentNullException(
                    "Specify an XPath search expression", "xpathExpression");
            }

            // Create an XML document
            var document = new XmlDocument();

            // Load the document
            document.Load(filePath);

            // Do the search
            var node = document.SelectSingleNode(xpathExpression);

            // Return the value back to the workflow
            context.SetValue(NodeValue, node == null ? VersioningHelper.PropertyNotFound : node.InnerText);
        }
    }
}