using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TfsBuild.Versioning.Activities.Tests
{
    [TestClass]
    public class TestInsertAssemblyVersion
    {
        [TestMethod]
        [DeploymentItem("TfsBuild.Versioning.Activities.Tests\\TestData\\ReplaceTestAssemblyInfo.fs")]
        public void TestInsertAssemblyVersion_WhenReplacingVersionsInFsFilesShouldPutDoAtTheEnd()
        {
            const string versionShell = "[<assembly: {0}(\"{1}\")>]";
            const string fileExtension = ".fs";
            const string newVersion = "1.2.3.4";
            const string doRegex = @".*(\(\s*\)|do\s*\(\s*\))";
            const string endOfFile = "do ()\r\n";

            var versionTitle = Enum.GetName(typeof(VersionTypeOptions), VersionTypeOptions.AssemblyVersion);
            var fileData = File.ReadAllText("ReplaceTestAssemblyInfo.fs");
            
            var result = ReplaceVersionInFile.InsertAssemblyVersion(fileExtension, versionTitle, fileData, newVersion);

            // "do" placed at the end of the file?
            Assert.IsTrue(result.EndsWith(endOfFile));

            // only one do
            var regex = new Regex(doRegex);
            Assert.AreEqual(1, regex.Matches(result).Count);
            
            // the version was placed in the file and only one of them
            var versionLine = string.Format(versionShell, versionTitle, newVersion);
            Assert.AreEqual(result.IndexOf(versionLine), result.LastIndexOf(versionLine));
        }
    }
}
