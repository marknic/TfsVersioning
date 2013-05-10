using System;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TfsBuild.Versioning.Activities.Tests.Stubs;

namespace TfsBuild.Versioning.Activities.Tests
{
    [TestClass]
    public class TestVersioningHelper
    {
        [TestMethod]
        public void TestVersioningHelper_WhenReplacingPatternsShouldWorkWithUpperOrLowerCase()
        {
            const string testString = "This is a test. Changing $YYYY and $b to their real values";
            var testDate = new DateTime(2011, 9, 18);

            IBuildDetail buildDetail = new BuildDetailStub(99);

            string updatedValue = VersioningHelper.ReplacePatternsInPropertyValue(testString, buildDetail, 0, testDate);

            var expectedResult = testString.Replace("$YYYY", "2011").Replace("$b", "99");

            Assert.AreEqual(expectedResult, updatedValue);
        }


        [TestMethod]
        public void TestVersioningHelper_WhenReplacingDatePatternsShouldDoUniversalLongDateAndShortDate()
        {
            const string testString = "$UTIME - $LDATE - $SDATE";
            var testDate = new DateTime(2011, 9, 18, 1, 20, 34, 200);

            IBuildDetail buildDetail = new BuildDetailStub(99);

            string updatedValue = VersioningHelper.ReplacePatternsInPropertyValue(testString, buildDetail, 0, testDate);

            const string expectedResult = "9/18/2011 6:20:34 AM - Sunday, September 18, 2011 - 9/18/2011";

            Assert.AreEqual(expectedResult, updatedValue);
        }

        [TestMethod]
        public void TestVersioningHelper_WhenReplacingDatePatternsShouldDoLongTimeShortDateAndShortTime()
        {
            const string testString = "$LTIME - $SDATE - $STIME";
            var testDate = new DateTime(2011, 9, 18, 1, 20, 34, 200);

            IBuildDetail buildDetail = new BuildDetailStub(99);

            string updatedValue = VersioningHelper.ReplacePatternsInPropertyValue(testString, buildDetail, 0, testDate);

            const string expectedResult = "1:20:34 AM - 9/18/2011 - 1:20 AM";

            Assert.AreEqual(expectedResult, updatedValue);
        }

        [TestMethod]
        public void TestVersioningHelper_WhenReplacingPatternsShouldHandleBuildNameLabelAndRequesteBy()
        {
            const string testString = "$bname - $reqby";
            var testDate = new DateTime(2011, 9, 18, 1, 20, 34, 200);

            IBuildDetail buildDetail = new BuildDetailStub(99);

            string updatedValue = VersioningHelper.ReplacePatternsInPropertyValue(testString, buildDetail, 0, testDate);

            const string expectedResult = "Build Name - marknic";

            Assert.AreEqual(expectedResult, updatedValue);
        }

        [TestMethod]
        public void TestVersioningHelper_WhenCallingProjectTypeFromFileNameShouldWorkWithKnownFileTypes()
        {
            string[] fileExtensions = new[] {"Cs", "Vb", "Cpp", "Fs"};
            
            foreach (var fileExtension in fileExtensions)
            {
                var filename = string.Format(@"D:\root\sub\testfilename.{0}", fileExtension);

                ProjectTypes projectType = VersioningHelper.GetProjectTypeFromFileName(filename);

                ProjectTypes expectedResult = (ProjectTypes) Enum.Parse(typeof (ProjectTypes), fileExtension);

                Assert.AreEqual(expectedResult, projectType);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestVersioningHelper_WhenCallingProjectTypeFromFileNameShouldThrowExceptionWithKnownFileTypes()
        {
            ProjectTypes projectType = VersioningHelper.GetProjectTypeFromFileName(@"D:\root\sub\filename.exe");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestVersioningHelper_WhenCallingProjectTypeFromFileNameShouldThrowExceptionWithNullFilename()
        {
            ProjectTypes projectType = VersioningHelper.GetProjectTypeFromFileName(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestVersioningHelper_WhenCallingProjectTypeFromFileNameShouldThrowExceptionWithNoExtension()
        {
            ProjectTypes projectType = VersioningHelper.GetProjectTypeFromFileName(@"D:\root\sub\filename");
        }

        [TestMethod]
        public void TestVersioningHelper_WhenPassingInDomainAndUserIdStripDomainShouldReturnJustTheUserId()
        {
            var result = VersioningHelper.StripDomain("DOMAIN\\UserId");

            Assert.AreEqual("UserId", result);
        }

        [TestMethod]
        public void TestVersioningHelper_WhenPassingInUserIdOnlyStripDomainShouldReturnTheUserId()
        {
            var result = VersioningHelper.StripDomain("UserId");

            Assert.AreEqual("UserId", result);
        }

    }
}
