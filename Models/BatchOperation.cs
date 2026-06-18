using System.Collections.Generic;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Represents a batch operation to perform on fabrication parts.
    /// The first three act on the current selection; the last two act on a set of
    /// spools chosen from the whole model (see <see cref="BatchOperation.SelectedSpools"/>).
    /// </summary>
    public enum BatchOperationType
    {
        ClearSpool,
        SetSpoolValue,
        ReportSummary,
        ExportMajBySpool,
        CreatePublishSetBySpool
    }

    /// <summary>
    /// Configuration for a batch operation.
    /// </summary>
    public class BatchOperation
    {
        public BatchOperationType OperationType { get; set; }
        public string TargetValue { get; set; } = "";

        /// <summary>Spool designations chosen for the spool-based operations.</summary>
        public List<string> SelectedSpools { get; set; } = new List<string>();

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
                BatchOperationType.ExportMajBySpool => $"Export {SelectedSpools.Count} spool(s) to MAJ",
                BatchOperationType.CreatePublishSetBySpool => $"Create publish set for {SelectedSpools.Count} spool(s)",
                _ => "Unknown"
            };
        }
    }
}
