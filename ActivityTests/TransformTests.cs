using System;
using NUnit.Framework;
using TfsBuild.AzureLM.Activities.Transform;
using TfsBuild.AzureLm.Activities;


namespace ActivityTests
{
    [TestFixture]
    public class TransformTests
    {
        [Test, Sequential]
        public void ConfigTransform_WhenPassingInGoodValuesShouldReturnProperFolder(
            [Values("C:\\folder\\file.config", "file.config", "file.config", "file.config", "file.config")] string source,
            [Values("folder", "folder", "", null, "\\folder\\sub\\")] string sourceRootPath,
            [Values("C:\\folder\\file.config", "folder\\file.config", "file.config", "file.config", "\\folder\\sub\\file.config")] string expectedResult
            )
        {
            var result = ConfigTransform.GetFilePathResolution(source, sourceRootPath);

            Assert.AreEqual(expectedResult, result);
        }

        [Test, Sequential, ExpectedException(typeof(ArgumentNullException))]
        public void ConfigTransform_WhenPassingInNullValuesShouldThrowException(
            [Values(null, null)] string source,
            [Values("folder", null)] string sourceRootPath
            )
        {
            ConfigTransform.GetFilePathResolution(source, sourceRootPath);
        }

        [Test, Sequential, ExpectedException(typeof(ArgumentNullException))]
        public void ConfigTransform_WhenPassingInNullValuesToDoTransformShouldThrowException(
            [Values(null,   "file", "file", null,   "file", null,   null)] string source,
            [Values("file", null,   "file", null,   null,   "file", null)] string transformFile,
            [Values("file", "file", null,   "file", null,   null,   null)] string destinationFile
            )
        {
            ConfigTransform.DoTransform(source, transformFile, destinationFile);
        }
    }
}
