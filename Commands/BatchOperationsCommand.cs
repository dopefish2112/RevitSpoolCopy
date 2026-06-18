using Autodesk.Revit.UI;

namespace RevitSpoolCopy.Commands
{
    /// <summary>
    /// Placeholder for "Batch Operations" command.
    /// Bulk operations on selected fabrication parts (clear spool, set values, etc).
    /// </summary>
    public class BatchOperationsCommand : ICommand
    {
        public string Id => "BatchOperations";
        public string DisplayName => "Batch\nOps";
        public string ToolTip => "[WIP] Bulk operations on selected parts.";
        public string LongDescription => "Clear Spool, set Spool to value, report summary.";

        public bool Execute(UIDocument uidoc, string message)
        {
            TaskDialog.Show("Batch Operations", "This feature is coming soon.");
            return false;
        }
    }
}
