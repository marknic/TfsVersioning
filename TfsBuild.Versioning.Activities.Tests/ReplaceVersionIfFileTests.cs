using System;
using System.Activities;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\ReplaceTestAssemblyInfo.cpp")]
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\ReplaceTestAssemblyInfo.vb")]
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\ReplaceTestAssemblyInfo.cs")]
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\ReplaceTestAssemblyInfo.fs")]
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\VersionUpdateTestData.xml")]
    [TestClass]
    public class ReplaceVersionInFileTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private const string SearchPatternShell = @"[\[<].*{0}.*\(\x22{1}\x22\)[\]>]+";

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\VersionUpdateTestData.xml", "versionChangeTest", DataAccessMethod.Sequential)]
        [TestMethod]
        public void ReplaceVersionInFile_WhenReplacingVersionShouldReplaceCorrectVersionNumber()
        {
            // Get the data from the XML file
            var filename = TestContext.DataRow["filename"].ToString();
            var version = TestContext.DataRow["version"].ToString();
            var versionType = (VersionTypeOptions) Enum.Parse(typeof(VersionTypeOptions), TestContext.DataRow["versionType"].ToString());
            var forceCreate = bool.Parse(TestContext.DataRow["forceCreate"].ToString());
            var expectedResult = bool.Parse(TestContext.DataRow["expectedResult"].ToString());
            var createVersionTypes = TestContext.DataRow["createVersionTypes"].ToString();

            // Convert the enum type into a string
            var versionName = versionType.ToString();

            var pattern = string.Format(SearchPatternShell, versionName, version);
            var regexPattern = pattern;

            // Create an instance of our test workflow
            var workflow = new Tests.TestReplaceVersionInFileWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            // Create a unique filename to store the test data file
            var rowNumber = TestContext.DataRow.Table.Rows.IndexOf(TestContext.DataRow);
            var testFilename = string.Format("{0}-{1}-{2}-{3}{4}", rowNumber, Path.GetFileNameWithoutExtension(filename), versionName,
                                 version, Path.GetExtension(filename));

            CreateTestFile(filename, testFilename, createVersionTypes);

            // Set the workflow arguments
            workflow.FilePath = testFilename;
            workflow.VersionType = VersionTypeOptions.AssemblyVersion;
            workflow.ForceCreate = forceCreate;
            workflow.ReplacementVersion = version;

            // Invoke the workflow and capture the outputs
            workflowInvoker.Invoke();

            // Verify using RegEx
            var fileData = File.ReadAllText(testFilename);

            var regex = new Regex(regexPattern);

            var matches = regex.Matches(fileData);

            // Assert the result based on expected results: should we find the version or not
            if (expectedResult)
            {
                Assert.IsTrue(matches.Count > 0, string.Format("Test Failed: File: {0}", testFilename));
            }
            else
            {
                Assert.AreEqual(0, matches.Count, string.Format("Test Failed: File: {0}", testFilename) );
            }
        }

        /// <summary>
        /// Create a temporary test file using the provided shell and adding version info to the file
        /// </summary>
        /// <param name="sourceFilename">File that contains a shell of an AssemblyInfo file for a particular language</param>
        /// <param name="targetFilename">Where to store the temporary test data file</param>
        /// <param name="createType">What to place in the file (AssemblyVersion, AssemblyFileVersion, neither or both)</param>
        private static void CreateTestFile(string sourceFilename, string targetFilename, string createType)
        {
            const string assemblyVersionCpp = "[assembly:AssemblyVersionAttribute(\"1.2.3.4\")];";
            const string assemblyFileVersionCpp = "[assembly:AssemblyFileVersionAttribute(\"1.2.3.4\")];";
            const string assemblyVersionCs = "[assembly: AssemblyVersion(\"2009.11.16.5\")]";
            const string assemblyFileVersionCs = "[assembly: AssemblyFileVersion(\"2009.11.16.5\")]";
            const string assemblyVersionVb = "<Assembly: AssemblyVersion(\"2009.11.16.5\")>";
            const string assemblyFileVersionVb = "<Assembly: AssemblyFileVersion(\"2009.11.16.5\")>";
            const string assemblyVersionFs = "[<assembly: AssemblyVersion(\"2009.11.16.5\")>]";
            const string assemblyFileVersionFs = "[<assembly: AssemblyFileVersion(\"2009.11.16.5\")>]";

            string assemblyVersion;
            string assemblyFileVersion;

            var fileDataBuilder = new StringBuilder();
            var extension = Path.GetExtension(sourceFilename).ToLower();

            fileDataBuilder.Append(File.ReadAllText(sourceFilename));
            fileDataBuilder.AppendLine();

            switch (extension)
            {
                case ".vb":
                    assemblyVersion = assemblyVersionVb;
                    assemblyFileVersion = assemblyFileVersionVb;
                    break;
                    
                case ".cpp":
                    assemblyVersion = assemblyVersionCpp;
                    assemblyFileVersion = assemblyFileVersionCpp;
                    break;

                case ".fs":
                    assemblyVersion = assemblyVersionFs;
                    assemblyFileVersion = assemblyFileVersionFs;
                    break;

                default:
                    assemblyVersion = assemblyVersionCs;
                    assemblyFileVersion = assemblyFileVersionCs;
                    break;
            }

            switch (createType.ToLower())
            {
                case "both":
                    fileDataBuilder.Append(assemblyVersion);
                    fileDataBuilder.AppendLine();
                    fileDataBuilder.Append(assemblyFileVersion);
                    fileDataBuilder.AppendLine();
                    break;
                
                case "av":
                    fileDataBuilder.Append(assemblyVersion);
                    fileDataBuilder.AppendLine();
                    break;

                case "afv":
                    fileDataBuilder.Append(assemblyFileVersion);
                    fileDataBuilder.AppendLine();
                    break;
            }

            // reset the do() binding in F# files
            if (extension == ".fs")
            {
                var fileData = fileDataBuilder.ToString();

                var regex = new Regex(@".*(\(\s*\)|do\s*\(\s*\))");

                fileData = regex.Replace(fileData, "");

                fileDataBuilder.Clear();

                fileDataBuilder.Append(fileData);
                fileDataBuilder.Append("do ()");
                fileDataBuilder.AppendLine();
            }

            File.WriteAllText(targetFilename, fileDataBuilder.ToString());
        }
    }
}
