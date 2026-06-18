namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Represents a batch operation to perform on fabrication parts.
    /// </summary>
    public enum BatchOperationType
    {
        ClearSpool,
        SetSpoolValue,
        ReportSummary
    }

    /// <summary>
    /// Configuration for a batch operation.
    /// </summary>
    public class BatchOperation
    {
        public BatchOperationType OperationType { get; set; }
        public string TargetValue { get; set; } = "";

        public BatchOperation() { }

        public BatchOperation(BatchOperationType type, string value = "")
        {
            OperationType = type;
            TargetValue = value;
        }

        public override string ToString()
        {
            return OperationType switch
            {
                BatchOperationType.ClearSpool => "Clear Spool",
                BatchOperationType.SetSpoolValue => $"Set Spool to '{TargetValue}'",
                BatchOperationType.ReportSummary => "Report Summary",
                _ => "Unknown"
            };
        }
    }
}
