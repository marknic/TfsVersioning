using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
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
    /// 
    /// </summary>
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\ReplaceTestAssemblyInfo.cpp")]
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\ReplaceTestAssemblyInfo.vb")]
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\ReplaceTestAssemblyInfo.cs")]
    [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\ReplaceTestAssemblyInfo.fs")]
    [TestClass]
    public class ReplaceAssemblyInfoPropertiesTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        //private const string SearchPatternShell = @"[\[<].*{0}.*\(\x22{1}\x22\)[\]>]+";
        [TestMethod]
        public void ReplaceAssemblyInfoPropertiesTests_WhenInsertingAssemblyInfoPropertiesShouldWorkAcrossAllProjectTypes()
        {
            const string description = "AssemblyInfo Property Value";
            const string assemblyInfoProperty = "AssemblyDescription";
            const string crLf = "\r\n";

            var assemblyStrings = new[] {VersioningHelper.CsAssembly, VersioningHelper.VbAssembly, VersioningHelper.CppAssembly, VersioningHelper.FsAssembly };
            var projectTypes = new[] {ProjectTypes.Cs, ProjectTypes.Vb, ProjectTypes.Cpp, ProjectTypes.Fs};

            for (var i = 0; i < assemblyStrings.Length; i++)
            {
                var fileData = string.Empty;

                var expectedResult = string.Format(crLf + assemblyStrings[i] + crLf, assemblyInfoProperty, description);

                var result = VersioningHelper.InsertAssemblyInfoProperty(fileData, assemblyInfoProperty, description, projectTypes[i]);

                Assert.AreEqual(expectedResult, result);
            }
        }

        [TestMethod]
        public void ReplaceAssemblyInfoPropertiesTests_WhenUpdatingAndPropertyDoesntExistWithForceOnShouldAddProperty()
        {
            const string description = "AssemblyInfo Property Value";
            const string assemblyInfoProperty = "AssemblyDescription";
            const string crLf = "\r\n";

            var assemblyStrings = new[] { VersioningHelper.CsAssembly, VersioningHelper.VbAssembly, VersioningHelper.CppAssembly, VersioningHelper.FsAssembly };
            var projectTypes = new[] { ProjectTypes.Cs, ProjectTypes.Vb, ProjectTypes.Cpp, ProjectTypes.Fs };

            for (var i = 0; i < assemblyStrings.Length; i++)
            {
                var fileData = string.Empty;

                var expectedResult = string.Format(crLf + assemblyStrings[i] + crLf, assemblyInfoProperty, description);

                var result = ReplaceAssemblyInfoProperties.UpdateAssemblyValue(fileData, assemblyInfoProperty, description, projectTypes[i], true);

                Assert.AreEqual(expectedResult, result);
            }
        }

        [TestMethod]
        public void ReplaceAssemblyInfoPropertiesTests_WhenUpdatingAndPropertyDoesntExistWithForceOffShouldNotAddProperty()
        {
            const string description = "AssemblyInfo Property Value";
            const string assemblyInfoProperty = "AssemblyDescription";
            var expectedResult = string.Empty;

            var assemblyStrings = new[] { VersioningHelper.CsAssembly, VersioningHelper.VbAssembly, VersioningHelper.CppAssembly, VersioningHelper.FsAssembly };
            var projectTypes = new[] { ProjectTypes.Cs, ProjectTypes.Vb, ProjectTypes.Cpp, ProjectTypes.Fs };

            for (var i = 0; i < assemblyStrings.Length; i++)
            {
                var fileData = string.Empty;

                var result = ReplaceAssemblyInfoProperties.UpdateAssemblyValue(fileData, assemblyInfoProperty, description, projectTypes[i], false);

                Assert.AreEqual(expectedResult, result);
            }
        }

        [TestMethod]
        public void ReplaceAssemblyInfoPropertiesTests_WhenUpdatingAndPropertyExistsShouldReplaceProperty()
        {
            const string description = "AssemblyInfo Property Value";
            const string assemblyInfoProperty = "AssemblyInformationalVersion";
            const string crLf = "\r\n";
            
            var assemblyStrings = new[] { VersioningHelper.CsAssembly, VersioningHelper.VbAssembly, VersioningHelper.CppAssembly, VersioningHelper.FsAssembly };
            var projectTypes = new[] { ProjectTypes.Cs, ProjectTypes.Vb, ProjectTypes.Cpp, ProjectTypes.Fs };

            for (var i = 0; i < assemblyStrings.Length; i++)
            {
                var fileData = SetupAssemblyInfo(projectTypes[i]);

                var expectedResult = string.Format(crLf + assemblyStrings[i] + crLf, assemblyInfoProperty, description);

                var result = ReplaceAssemblyInfoProperties.UpdateAssemblyValue(fileData, assemblyInfoProperty, description, projectTypes[i], false);

                Assert.AreEqual(expectedResult, result);
            }
        }

        private static string SetupAssemblyInfo(ProjectTypes projectType)
        {
            switch (projectType)
            {
                case ProjectTypes.Cs:
                    return "\r\n[assembly: AssemblyInformationalVersion(\"This is some info\")]\r\n";
                   
                case ProjectTypes.Vb:
                    return "\r\n<Assembly: AssemblyInformationalVersion(\"This is some info\")>\r\n";
                    
                case ProjectTypes.Cpp:
                    return "\r\n[assembly: AssemblyInformationalVersionAttribute(\"This is some info\")];\r\n";                    
                    
                case ProjectTypes.Fs:
                    return "\r\n[<assembly: AssemblyInformationalVersion(\"This is some info\")>]\r\n";
                    
            }

            throw new MSTestInvalidArgumentException();
        }

        [TestMethod]
        public void ReplaceAssemblyInfoPropertiesTests_WhenWorkingWithFsFileShouldRealignOrInsertDoStatementAtEnd()
        {
            string[] filePaths = new[] { "ReplaceTestAssemblyInfo.cs", "ReplaceTestAssemblyInfo.vb", "ReplaceTestAssemblyInfo.cpp", "ReplaceTestAssemblyInfo.fs" };

            foreach (var file in filePaths)
            {
                // Create an instance of our test workflow
                var workflow = new Tests.TestReplaceAssemblyInfoPropertiesWorkflow();

                ProjectTypes projectType = VersioningHelper.GetProjectTypeFromFileName(file);

                // Create the workflow run-time environment
                var workflowInvoker = new WorkflowInvoker(workflow);

                var testFilename = string.Format("TestingDoInclusionAssemblyInfo{0}", Path.GetExtension(file));

                File.Copy(file, testFilename);

                // Set the workflow arguments
                workflow.FilePath = testFilename;
                workflow.ForceCreate = true;

                workflow.BuildDate = DateTime.Now;
                workflow.AssemblyTitleReplacementPattern = string.Empty;
                workflow.AssemblyDescriptionReplacementPattern = string.Empty;
                workflow.AssemblyConfigurationReplacementPattern = string.Empty;
                workflow.AssemblyCompanyReplacementPattern = string.Empty;
                workflow.AssemblyProductReplacementPattern = string.Empty;
                workflow.AssemblyCopyrightReplacementPattern = string.Empty;
                workflow.AssemblyTrademarkReplacementPattern = string.Empty;
                workflow.AssemblyCultureReplacementPattern = string.Empty;
                workflow.AssemblyInformationalVersionReplacementPattern = "$TPROJ : $REQBY : $BNAME : $UTIME : $LDATE : $LTIME : $SDATE : $STIME : $BNUM : $YYYY : $YY : $MM : $M : $DD : $D : $J : $B";
                workflow.BuildDetail = new InArgument<IBuildDetail>(env => new BuildDetailStub(99));
                
                // Invoke the workflow and capture the outputs
                workflowInvoker.Invoke();

                // Verify using RegEx
                var fileData = File.ReadAllText(testFilename);

                var regex = new Regex(@".*(\(\s*\)|do\s*\(\s*\))");

                if (projectType == ProjectTypes.Fs)
                {
                    Assert.IsTrue(regex.IsMatch(fileData));
                }
                else
                {
                    Assert.IsFalse(regex.IsMatch(fileData));
                }
            }
        }

        [TestMethod]
        public void ReplaceAssemblyInfoPropertiesTests_WhenChangingAllPropertiesInWorkflowShouldUpdateAssemblyInfoWithSameValues()
        {
            const string searchPatternShell = @"[\[<]+.*{0}.*\(\x22{1}\x22\)[\]>]+";
            string[] filePaths = new[] { "ReplaceTestAssemblyInfo.cs", "ReplaceTestAssemblyInfo.vb", "ReplaceTestAssemblyInfo.cpp", "ReplaceTestAssemblyInfo.fs" };

            const bool forceCreate = true;
            
            IList<Tuple<string, string>> assemblyProperties = new ReadOnlyCollectionBuilder<Tuple<string, string>>
                                                                  {
                                                                      new Tuple<string, string>("AssemblyTitle", "Assembly Title"),
                                                                      new Tuple<string, string>("AssemblyDescription", "Assembly Description"),
                                                                      new Tuple<string, string>("AssemblyConfiguration", "Assembly Configuration"),
                                                                      new Tuple<string, string>("AssemblyCompany", "Assembly Company"),
                                                                      new Tuple<string, string>("AssemblyProduct", "Assembly Product"),
                                                                      new Tuple<string, string>("AssemblyCopyright", "Assembly Copyright"),
                                                                      new Tuple<string, string>("AssemblyTrademark", "Assembly Trademark"),
                                                                      new Tuple<string, string>("AssemblyCulture", "Assembly Culture"),
                                                                      new Tuple<string, string>("AssemblyInformationalVersion", "Assembly Informational Version"),
                                                                  };

            foreach (var file in filePaths)
            {
                // Create an instance of our test workflow
                var workflow = new Tests.TestReplaceAssemblyInfoPropertiesWorkflow();

                // Create the workflow run-time environment
                var workflowInvoker = new WorkflowInvoker(workflow);

                var testFilename = string.Format("AllPropertiesTestAssemblyInfo{0}", Path.GetExtension(file));

                File.Copy(file, testFilename);

                var extension = " " + Path.GetExtension(testFilename).Remove(0, 1);

                // Set the workflow arguments
                workflow.FilePath = testFilename;
                workflow.ForceCreate = forceCreate;

                workflow.BuildDate = DateTime.Now;
                workflow.AssemblyTitleReplacementPattern = assemblyProperties[0].Item2 + extension;
                workflow.AssemblyDescriptionReplacementPattern = assemblyProperties[1].Item2 + extension;
                workflow.AssemblyConfigurationReplacementPattern = assemblyProperties[2].Item2 + extension;
                workflow.AssemblyCompanyReplacementPattern = assemblyProperties[3].Item2 + extension;
                workflow.AssemblyProductReplacementPattern = assemblyProperties[4].Item2 + extension;
                workflow.AssemblyCopyrightReplacementPattern = assemblyProperties[5].Item2 + extension;
                workflow.AssemblyTrademarkReplacementPattern = assemblyProperties[6].Item2 + extension;
                workflow.AssemblyCultureReplacementPattern = assemblyProperties[7].Item2 + extension;
                workflow.AssemblyInformationalVersionReplacementPattern = assemblyProperties[8].Item2 + extension;
                workflow.BuildDetail = new InArgument<IBuildDetail>(env => new BuildDetailStub(99));

                // Invoke the workflow and capture the outputs
                workflowInvoker.Invoke();

                // Verify using RegEx
                var fileData = File.ReadAllText(testFilename);

                foreach (var propertyTuple in assemblyProperties)
                {
                    var regexPattern = string.Format(searchPatternShell, propertyTuple.Item1,
                                                     propertyTuple.Item2 + extension);

                    var regex = new Regex(regexPattern);

                    Assert.IsTrue(regex.IsMatch(fileData),
                                  string.Format("Test Failed: File: {0} - Property: {1} - Value: {2}", testFilename,
                                                propertyTuple.Item1, propertyTuple.Item2 + extension));
                }
            }
        }



        [TestMethod]
        public void ReplaceAssemblyInfoPropertiesTests_WhenChangingSelectedPropertiesInWorkflowShouldOnlyUpdateSelectedAssemblyInfoValues()
        {
            const string searchPatternShell = @"[\[<]+.*{0}.*\(\x22{1}\x22\)[\]>]+";
            string[] filePaths = new[] { "ReplaceTestAssemblyInfo.cs", "ReplaceTestAssemblyInfo.vb", "ReplaceTestAssemblyInfo.cpp", "ReplaceTestAssemblyInfo.fs" };

            const bool forceCreate = true;

            IList<Tuple<string, string>> assemblyProperties = new ReadOnlyCollectionBuilder<Tuple<string, string>>
                                                                  {
                                                                      new Tuple<string, string>("AssemblyTitle", "Assembly Title"),
                                                                      new Tuple<string, string>("AssemblyDescription", "Assembly Description"),
                                                                      new Tuple<string, string>("AssemblyConfiguration", "Assembly Configuration"),
                                                                      new Tuple<string, string>("AssemblyCompany", "Assembly Company"),
                                                                      new Tuple<string, string>("AssemblyProduct", "Assembly Product"),
                                                                      new Tuple<string, string>("AssemblyCopyright", null),
                                                                      new Tuple<string, string>("AssemblyTrademark", "Assembly Trademark"),
                                                                      new Tuple<string, string>("AssemblyCulture", "Assembly Culture"),
                                                                      new Tuple<string, string>("AssemblyInformationalVersion", string.Empty),
                                                                  };

            foreach (var file in filePaths)
            {
                // Create an instance of our test workflow
                var workflow = new Tests.TestReplaceAssemblyInfoPropertiesWorkflow();

                // Create the workflow run-time environment
                var workflowInvoker = new WorkflowInvoker(workflow);

                var testFilename = string.Format("SelectedPropertiesTestAssemblyInfo{0}", Path.GetExtension(file));

                File.Copy(file, testFilename);

                var extension = " " + Path.GetExtension(testFilename).Remove(0, 1);

                // Set the workflow arguments
                workflow.FilePath = testFilename;
                workflow.ForceCreate = forceCreate;

                workflow.AssemblyTitleReplacementPattern = assemblyProperties[0].Item2 + extension;
                workflow.AssemblyDescriptionReplacementPattern = assemblyProperties[1].Item2 + extension;
                workflow.AssemblyConfigurationReplacementPattern = assemblyProperties[2].Item2 + extension;
                workflow.AssemblyCompanyReplacementPattern = assemblyProperties[3].Item2 + extension;
                workflow.AssemblyProductReplacementPattern = assemblyProperties[4].Item2 + extension;
                workflow.AssemblyCopyrightReplacementPattern = null;
                workflow.AssemblyTrademarkReplacementPattern = assemblyProperties[6].Item2 + extension;
                workflow.AssemblyCultureReplacementPattern = assemblyProperties[7].Item2 + extension;
                workflow.AssemblyInformationalVersionReplacementPattern = string.Empty;

                // Invoke the workflow and capture the outputs
                workflowInvoker.Invoke();

                // Verify using RegEx
                var fileData = File.ReadAllText(testFilename);

                foreach (var propertyTuple in assemblyProperties)
                {
                    var regexPattern = string.Format(searchPatternShell, propertyTuple.Item1,
                                                     propertyTuple.Item2 + extension);

                    var regex = new Regex(regexPattern);

                    if (string.IsNullOrWhiteSpace(propertyTuple.Item2))
                    {
                        Assert.IsFalse(regex.IsMatch(fileData),
                                  string.Format("Test Failed (should not match): File: {0} - Property: {1} - Value: {2}", testFilename,
                                                propertyTuple.Item1, propertyTuple.Item2 + extension));
                    }
                    else
                    {
                        Assert.IsTrue(regex.IsMatch(fileData),
                                      string.Format("Test Failed (should match): File: {0} - Property: {1} - Value: {2}", testFilename,
                                                    propertyTuple.Item1, propertyTuple.Item2 + extension));
                    }
                }
            }
        }

        [TestMethod]
        public void ReplaceAssemblyInfoPropertiesTests_WhenNewLineIsAtEndOfFileShouldReturnTrue()
        {
            var testString = "Line 1\r\nLine 2\r\n";

            bool result = ReplaceAssemblyInfoProperties.DoesLastLineContainCr(testString);

            Assert.IsTrue(result);

            testString = "Line 1\r\nLine 2\n";

            result = ReplaceAssemblyInfoProperties.DoesLastLineContainCr(testString);

            Assert.IsTrue(result);

            testString = string.Format("Line 1\r\nLine 2{0}{0}{0}", Environment.NewLine);

            result = ReplaceAssemblyInfoProperties.DoesLastLineContainCr(testString);

            Assert.IsTrue(result);

            testString = "Line 1\r\nLine 2";

            result = ReplaceAssemblyInfoProperties.DoesLastLineContainCr(testString);

            Assert.IsFalse(result);
        }   
    }
}
