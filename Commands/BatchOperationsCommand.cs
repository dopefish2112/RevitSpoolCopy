using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitSpoolCopy.Models;
using RevitSpoolCopy.UI;

namespace RevitSpoolCopy.Commands
{
    /// <summary>
    /// Command: Perform bulk operations on selected fabrication parts.
    /// Operations: clear spool, set spool to value, report summary.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class BatchOperationsCommand : ICommand, IExternalCommand
    {
        public string Id => "BatchOperations";
        public string DisplayName => "Batch\nOps";
        public string ToolTip => "Bulk operations on selected parts (clear, set, report).";
        public string LongDescription => "Clear Spool, set Spool to value, or report summary of selected fabrication parts.";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Logger.Info($"Execute '{Id}' begin");
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                string msg = "";
                bool success = Execute(uidoc, msg);
                Logger.Info($"Execute '{Id}' end -> {(success ? "Succeeded" : "Failed")}");
                return success ? Result.Succeeded : Result.Failed;
            }
            catch (Exception ex)
            {
                Logger.Error($"{Id}.Execute", ex);
                message = $"Error: {ex.Message}\n\nDetails logged to:\n{Logger.LogFilePath}";
                return Result.Failed;
            }
        }

        public bool Execute(UIDocument uidoc, string message)
        {
            if (uidoc == null)
            {
                message = "No active document.";
                return false;
            }

            Document doc = uidoc.Document;

            // Get selected fabrication parts
            var elementIds = uidoc.Selection.GetElementIds();
            if (elementIds.Count == 0)
            {
                TaskDialog.Show("Batch Operations", "Select one or more fabrication parts first.");
                return false;
            }

            var parts = FabricationPartHelper.FilterFabricationParts(elementIds, doc);
            if (parts.Count == 0)
            {
                TaskDialog.Show("Batch Operations",
                    "No fabrication parts in selection. Select MEP fabrication parts and retry.");
                return false;
            }

            // Show dialog to select operation
            var dialog = new BatchOperationsDialog();
            bool? result = dialog.ShowDialog();

            if (result != true)
                return false; // User cancelled

            var operation = dialog.GetSelectedOperation();
            if (operation == null)
                return false;

            // Execute operation
            switch (operation.OperationType)
            {
                case BatchOperationType.ClearSpool:
                    return ExecuteClearSpool(doc, parts);
                case BatchOperationType.SetSpoolValue:
                    return ExecuteSetSpool(doc, parts, operation.TargetValue);
                case BatchOperationType.ReportSummary:
                    return ExecuteReportSummary(parts);
                default:
                    return false;
            }
        }

        private bool ExecuteClearSpool(Document doc, List<FabricationPart> parts)
        {
            int cleared = 0;

            using (var t = new Transaction(doc, "Clear Spool"))
            {
                t.Start();
                foreach (var part in parts)
                {
                    try
                    {
                        part.SpoolName = "";
                        cleared++;
                    }
                    catch
                    {
                        // Skip parts that can't be modified
                    }
                }
                t.Commit();
            }

            TaskDialog.Show("Clear Spool", $"Cleared spool on {cleared} of {parts.Count} parts.");
            return true;
        }

        private bool ExecuteSetSpool(Document doc, List<FabricationPart> parts, string value)
        {
            int set = 0;

            using (var t = new Transaction(doc, "Set Spool"))
            {
                t.Start();
                foreach (var part in parts)
                {
                    try
                    {
                        part.SpoolName = value;
                        set++;
                    }
                    catch
                    {
                        // Skip parts that can't be modified
                    }
                }
                t.Commit();
            }

            TaskDialog.Show("Set Spool", $"Set spool to '{value}' on {set} of {parts.Count} parts.");
            return true;
        }

        private bool ExecuteReportSummary(List<FabricationPart> parts)
        {
            var spoolGroups = parts
                .GroupBy(p => p.SpoolName ?? "(empty)")
                .OrderBy(g => g.Key)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"Summary of {parts.Count} Fabrication Parts:");
            sb.AppendLine();

            foreach (var group in spoolGroups)
            {
                sb.AppendLine($"  Spool '{group.Key}': {group.Count()} part{(group.Count() > 1 ? "s" : "")}");
            }

            sb.AppendLine();
            sb.AppendLine($"Total: {spoolGroups.Count} unique spool value{(spoolGroups.Count > 1 ? "s" : "")}");

            TaskDialog.Show("Report Summary", sb.ToString());
            return true;
        }
    }
}

