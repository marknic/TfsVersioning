using System;
using System.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ==============================================================================================
// http://tfsversioning.codeplex.com/
//
// Author: Mark S. Nichols
//
// Copyright (c) 2011 Microsoft Corporation
//
// This source is subject to the Microsoft Permissive License. 
// ==============================================================================================

namespace TfsBuild.Versioning.Activities.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class XmlExtractTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\VersionSeed.xml")]
        public void XmlExtract_WhenUsingActivityWithGoodProjectNameShouldExtractAssemblyVersionPattern()
        {
            // Create an instance of our test workflow
            var workflow = new Tests.TestXmlExtractWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            workflow.XmlFilePath = "VersionSeed.xml";
            workflow.SolutionName = "TfsBuild.Versioning.Activities";

            // Invoke the workflow and capture the outputs
            var outputs = workflowInvoker.Invoke();

            // Retrieve the out arguments to do our verification
            var assemblyVersionPattern = (String)outputs["AssemblyVersionPattern"];
            var assemblyFileVersionPattern = (String)outputs["AssemblyFileVersionPattern"];

            // Verify that we captured the version component of the build number
            Assert.AreEqual("YYYY.M.D.b", assemblyVersionPattern);
            Assert.AreEqual("1.2.j.b", assemblyFileVersionPattern);
        }

        [TestMethod]
        [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\VersionSeed.xml")]
        public void XmlExtract_WhenExtractingPatternWithMisMatchedProjectNameShouldExtractDefaultPattern()
        {
            // Create an instance of our test workflow
            var workflow = new TfsBuild.Versioning.Activities.Tests.TestXmlExtractWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            workflow.XmlFilePath = "VersionSeed.xml";
            workflow.SolutionName = "TfsBuild.Versioning.Activities";

            // Invoke the workflow and capture the outputs
            var outputs = workflowInvoker.Invoke();

            // Retrieve the out arguments to do our verification
            var assemblyVersionPattern = (String)outputs["AssemblyVersionPattern"];
            var assemblyFileVersionPattern = (String)outputs["AssemblyFileVersionPattern"];


            // Verify that we captured the version component of the build number
            Assert.AreEqual("YYYY.M.D.b", assemblyVersionPattern);
            Assert.AreEqual("1.2.j.b", assemblyFileVersionPattern);
        }

    }
}
