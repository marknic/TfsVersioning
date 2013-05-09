using NUnit.Framework;
using TfsBuild.AzureLM.Activities.Models;
using TfsBuild.AzureLm.Activities.Enumerations;
using TfsBuild.AzureLm.Activities.Utility;


namespace ActivityTests
{
    [TestFixture]
    public class DeploymentSupportTests
    {
        [Test]
        public void DeploymentSupportTests_WhenEncodingBase64ShouldDecodePropertly()
        {
            const string testString = "This is a test to verify encoding and decoding =+-_$@#";

            var codedString = DeploymentSupport.EncodeToBase64String(testString);
            var decodedString = DeploymentSupport.DecodeFromBase64String(codedString);

            Assert.AreEqual(testString, decodedString);
        }

        [Test, Sequential]
        public void WorkflowDecoderWhenDecodingWithTestDataShouldEvaluateToExpectedResultColumn(
            [Values(0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7)] int target,
            [Values(true, true, false, false, true, true, false, false, true, true, false, false, true, true, false, false, true, true, false, false, true, true, false, false, true, true, false, false, true, true, false, false)] bool productionExists,
            [Values(true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false)] bool stagingExists,
            [Values("NFFF", "NFFF", "NFFF", "NFFF", "NTFF", "NTFF", "PFFF", "PFFF", "PFTF", "PFTF", "PFFF", "PFFF", "NTFF", "SFFF", "NTFF", "SFFF", "SFTF", "SFFF", "SFTF", "SFFF", "NTFF", "SFFT", "PFFF", "PFFF", "SFTT", "SFFT", "PFFF", "PFFF", "NFFF", "NFFF", "NFFF", "NFFF")] string expectedResults
            )
        {
            DeploymentSlots expDeploymentSlot;

            switch (expectedResults[0])
            {
                case 'S':
                    expDeploymentSlot = DeploymentSlots.Staging;
                    break;

                case 'P':
                    expDeploymentSlot = DeploymentSlots.Production;
                    break;

                default:
                    expDeploymentSlot = DeploymentSlots.None;
                    break;
            }

            var expDeploymentInWay = expectedResults[1] == 'T';
            var expDoDelete = expectedResults[2] == 'T';
            var expDoSwap = expectedResults[3] == 'T';

            var expectedResult = new DeploymentWorkflowIndicators
                {
                    DeploymentInWay = expDeploymentInWay,
                    DeploymentSlot = expDeploymentSlot,
                    DoDelete = expDoDelete,
                    DoSwap = expDoSwap
                };

            var result = DeploymentSupport.DecodeWorkflowData((DeploymentWorkflowTargets)target, stagingExists, productionExists);

            Assert.IsTrue(expectedResult.Equals(result));

        }
    }
}
