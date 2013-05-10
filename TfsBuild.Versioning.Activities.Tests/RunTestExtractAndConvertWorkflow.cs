using System;
using System.Activities;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsBuild.Versioning.Activities.Tests.Stubs;

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
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\WrapperTestAssemblyInfo.cpp")]
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\WrapperTestAssemblyInfo.vb")]
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\WrapperTestAssemblyInfo.cs")]
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\VersionSeed.xml")]
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\ReplaceCorrectVersion.csv")]
    [TestClass]
    public class RunTestExtractAndConvertWorkflow
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private const string SearchPatternShell = @"[\[<].*{0}.*\(\x22[(\d\.)\d]+\x22\)[\]>]";

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\ReplaceCorrectVersion.csv", "ReplaceCorrectVersion#csv", DataAccessMethod.Sequential)]
        [TestMethod]
        public void RunTestExtractAndConvertWorkflow_WhenRunningFullWorkflowShouldReplaceCorrectVersionNumbers()
        {
            // Create an instance of our test workflow
            var workflow = new TestExtractAndConvertWorkflowWrapper();
            
            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);
            
            var minuteCount = (int)DateTime.Now.TimeOfDay.TotalMinutes;

            workflowInvoker.Extensions.Add(new BuildDetailStub(minuteCount));

            // Set the workflow arguments
            workflow.ForceCreateVersion = true;
            workflow.AssemblyFileVersionReplacementPattern = "YYYY.MM.DD.B";
            workflow.BuildNumber = "TestCodeActivity - 2_20110310.3";
            workflow.AssemblyVersionReplacementPattern = "1.2.3.4";
            workflow.FolderToSearch = TestContext.DeploymentDirectory;
            workflow.FileSearchPattern = "AssemblyInfo.*";
            workflow.BuildNumberPrefix = 0;

            // Invoke the workflow and capture the outputs
            workflowInvoker.Invoke();

            var file = TestContext.DataRow[0].ToString();
            var versionName = TestContext.DataRow[1].ToString();
            
            var fileData = File.ReadAllText(file);
            var regexPattern = string.Format(SearchPatternShell, versionName);
            var regex = new Regex(regexPattern);
            var matches = regex.Matches(fileData);

            Assert.AreEqual(1, matches.Count);
        }
    }
}
