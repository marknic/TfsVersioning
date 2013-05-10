using System;
using System.Activities;
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
    [TestClass]
    public class ConvertVersionPatternTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ConvertVersionPattern_WhenUsingYYYYMDBShouldConvertIntoProperVersion()
        {
            var totalHours = (int)DateTime.Now.TimeOfDay.TotalHours;

            // Create an instance of our test workflow
            var workflow = new Tests.ConvertVersionPatternTestWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            workflowInvoker.Extensions.Add(new BuildDetailStub(totalHours));

            workflow.VersionPattern = "yyyy.MM.DD.B";
            
            // Invoke the workflow and capture the outputs
            var outputs = workflowInvoker.Invoke();

            // Retrieve the out arguments to do our verification
            var convertedVersionNumber = (String)outputs["ConvertedVersionNumber"];

            // Verify that we captured the version component of the build number
            Assert.AreEqual(string.Format("{0}.{1}.{2}.{3}", DateTime.Now.Year, DateTime.Now.Month,
                                          DateTime.Now.Day, Convert.ToInt16(totalHours)),
                            convertedVersionNumber);
        }

        [TestMethod]
        public void ConvertVersionPattern_WhenUsingMajorMinorJulianBuildShouldConvertIntoProperVersion()
        {
            var totalHours = (int)DateTime.Now.TimeOfDay.TotalHours;

            // Create an instance of our test workflow
            var workflow = new Tests.ConvertVersionPatternTestWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            workflowInvoker.Extensions.Add(new BuildDetailStub(totalHours));

            workflow.VersionPattern = "1.2.J.B";

            // Invoke the workflow and capture the outputs
            var outputs = workflowInvoker.Invoke();

            // Retrieve the out arguments to do our verification
            var convertedVersionNumber = (String)outputs["ConvertedVersionNumber"];

            // Verify that we captured the version component of the build number
            Assert.AreEqual(string.Format("{0}.{1}.{2}.{3}", 1, 2, 
                String.Format("{0}{1}", DateTime.Now.ToString("yy"), string.Format("{0:000}", DateTime.Now.DayOfYear)),
                Convert.ToInt16(totalHours)), convertedVersionNumber);
        }


        [TestMethod]
        public void ConvertVersionPattern_WhenUsingMajorMinorJulianPartialShouldConvertIntoProperVersion()
        {
            var totalHours = (int)DateTime.Now.TimeOfDay.TotalHours;

            // Create an instance of our test workflow
            var workflow = new Tests.ConvertVersionPatternTestWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            workflowInvoker.Extensions.Add(new BuildDetailStub(totalHours));

            workflow.VersionPattern = "1.2.J";

            // Invoke the workflow and capture the outputs
            var outputs = workflowInvoker.Invoke();

            // Retrieve the out arguments to do our verification
            var convertedVersionNumber = (String)outputs["ConvertedVersionNumber"];

            // Verify that we captured the version component of the build number
            Assert.AreEqual(string.Format("{0}.{1}.{2}", 1, 2, 
                String.Format("{0}{1}", DateTime.Now.ToString("yy"), string.Format("{0:000}", DateTime.Now.DayOfYear))), convertedVersionNumber);
        }


        [TestMethod]
        public void ConvertVersionPattern_WhenUsing_MajorMinor_PartialShouldConvertIntoProperVersion()
        {
            var totalHours = (int)DateTime.Now.TimeOfDay.TotalHours;

            // Create an instance of our test workflow
            var workflow = new Tests.ConvertVersionPatternTestWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            workflowInvoker.Extensions.Add(new BuildDetailStub(totalHours));

            workflow.VersionPattern = "1.2";
            
            // Invoke the workflow and capture the outputs
            var outputs = workflowInvoker.Invoke();

            // Retrieve the out arguments to do our verification
            var convertedVersionNumber = (String)outputs["ConvertedVersionNumber"];

            // Verify that we captured the version component of the build number
            Assert.AreEqual("1.2", convertedVersionNumber);
        }


        [TestMethod]
        public void ConvertVersionPattern_WhenUsingBuildPrefixShouldConvertIntoProperVersion()
        {
            var totalHours = (int)DateTime.Now.TimeOfDay.TotalHours;
            const int prefixVal = 100;

            // Create an instance of our test workflow
            var workflow = new Tests.ConvertVersionPatternTestWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            workflowInvoker.Extensions.Add(new BuildDetailStub(totalHours));

            workflow.VersionPattern = "1.0.0.B";

            workflow.BuildNumberPrefix = prefixVal;

            // Invoke the workflow and capture the outputs
            var outputs = workflowInvoker.Invoke();

            // Retrieve the out arguments to do our verification
            var convertedVersionNumber = (String)outputs["ConvertedVersionNumber"];

            // Verify that we captured the version component of the build number
            Assert.AreEqual(string.Format("1.0.0.{0}", totalHours + prefixVal), convertedVersionNumber);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConvertVersionPattern_WhenUsingBuildPrefixShouldThrowExceptionIfBuildNumberGreaterThanPrefix()
        {
            // Create an instance of our test workflow
            var workflow = new Tests.ConvertVersionPatternTestWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            workflowInvoker.Extensions.Add(new BuildDetailStub(10));

            workflow.VersionPattern = "1.0.0.B";

            workflow.BuildNumberPrefix = 10;

            // Invoke the workflow and capture the outputs
            workflowInvoker.Invoke();
        }

        [TestMethod]
        public void ConvertVersionPattern_WhenUsing_YYYY_PartialShouldConvertIntoProperVersion()
        {
            var totalHours = (int)DateTime.Now.TimeOfDay.TotalHours;

            // Create an instance of our test workflow
            var workflow = new Tests.ConvertVersionPatternTestWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            workflowInvoker.Extensions.Add(new BuildDetailStub(totalHours));

            workflow.VersionPattern = "YYYY";

            // Invoke the workflow and capture the outputs
            var outputs = workflowInvoker.Invoke();

            // Retrieve the out arguments to do our verification
            var convertedVersionNumber = (String)outputs["ConvertedVersionNumber"];

            // Verify that we captured the version component of the build number
            Assert.AreEqual(string.Format("{0}", DateTime.Now.ToString("yyyy")), convertedVersionNumber);
        }
    }
}
