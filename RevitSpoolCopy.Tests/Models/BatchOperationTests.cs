using RevitSpoolCopy.Models;

namespace RevitSpoolCopy.Tests.Models
{
    public class BatchOperationTests
    {
        [Fact]
        public void Ctor_SetsTypeAndValue()
        {
            var op = new BatchOperation(BatchOperationType.SetSpoolValue, "SP-01");

            Assert.Equal(BatchOperationType.SetSpoolValue, op.OperationType);
            Assert.Equal("SP-01", op.TargetValue);
        }

        [Fact]
        public void Ctor_Default_HasEmptyTargetValue()
        {
            var op = new BatchOperation(BatchOperationType.ClearSpool);

            Assert.Equal("", op.TargetValue);
        }

        [Theory]
        [InlineData(BatchOperationType.ClearSpool, "", "Clear Spool")]
        [InlineData(BatchOperationType.SetSpoolValue, "SP-01", "Set Spool to 'SP-01'")]
        [InlineData(BatchOperationType.ReportSummary, "", "Report Summary")]
        public void ToString_DescribesOperation(BatchOperationType type, string value, string expected)
        {
            var op = new BatchOperation(type, value);

            Assert.Equal(expected, op.ToString());
        }
    }
}
