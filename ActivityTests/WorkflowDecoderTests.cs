using NUnit.Framework;
using TfsBuild.AzureLm.Activities;


namespace ActivityTests
{
    [TestFixture]
    public class WorkflowDecoderTests
    {
        [Test, Sequential]
        public void WorkflowDecoderWhenDecodingWithTestDataShouldEvaluateToExpectedResultColumn(
            [Values("StagingNoDelete", "stagingnodelete", "stagingNoDelete", "StagingNoDelete", "StaginNoDelete", "StagNoDelete", "StaNoDelete", "stadelete", "stagdelet", "stagdelete", "StagingDelete", "Stagindelete", "stagingdeleteno", "ProductionNoDelete", "productionnodelete", "productiNoDelete", "productionoDelete", "productNoDelete", "prodNoDelete", "proNoDelete", "prodelete", "proddelet", "ProductionDelete", "produtciondelete", "productiondeleteno", "prnodelete", "ProductionDemoteNoDelete", "productiondemotenodelete", "productiDemoteNoDelete", "productiodemotenoDelete", "productNoDeletedemote", "proddemoteNoDelete", "proNoDeletedemote", "prodemotedelete", "proddemotedelet", "proddeletedemote", "ProductionDemoteDelete", "produtciondemotedelete", "productiondeletenodemote", "prdemotenodelete")] string patternToTest,
            [Values("StagingNoDelete", "StagingNoDelete", "StagingNoDelete", "StagingNoDelete", "StagingNoDelete", "StagingNoDelete", "null",        "null",      "StagingNoDelete", "StagingDelete", "StagingDelete", "StagingDelete", "StagingNoDelete", "ProductionNoDelete", "ProductionNoDelete", "ProductionNoDelete", "ProductionNoDelete", "ProductionNoDelete", "ProductionNoDelete", "null",        "ProductionDelete", "ProductionNoDelete", "ProductionDelete", "ProductionDelete", "ProductionNoDelete", "null",       "ProductionDemoteNoDelete", "ProductionDemoteNoDelete", "ProductionDemoteNoDelete", "ProductionDemoteNoDelete", "ProductionDemoteNoDelete", "ProductionDemoteNoDelete", "null",              "ProductionDemoteDelete", "ProductionDemoteNoDelete", "ProductionDemoteDelete", "ProductionDemoteDelete", "ProductionDemoteDelete", "ProductionDemoteNoDelete", "null")] string expectedResultData
            )
        {
            var expectedResult = expectedResultData.ToLower().Trim().Equals("null") ? null : expectedResultData;

            var workflowDecoder = new WorkflowDecoder();

            var result = workflowDecoder.Execute(patternToTest);

            Assert.AreEqual(expectedResult, result);
        }
    }
}
