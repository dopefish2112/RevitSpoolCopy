using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
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

            // Selected parts (for the selection-based operations; may be empty).
            var selectedParts = FabricationPartHelper.FilterFabricationParts(
                uidoc.Selection.GetElementIds(), doc);

            // All fabrication parts in the model, grouped by spool (for the spool-based ops).
            var allParts = new FilteredElementCollector(doc)
                .OfClass(typeof(FabricationPart))
                .Cast<FabricationPart>()
                .ToList();

            var spoolGroups = allParts
                .GroupBy(p => SpoolExportLogic.NormalizeSpool(p.SpoolName))
                .OrderBy(g => g.Key)
                .Select(g => new SpoolInfo { SpoolName = g.Key, PartCount = g.Count() })
                .ToList();

            // Show dialog to select operation
            var dialog = new BatchOperationsDialog(spoolGroups);
            bool? result = dialog.ShowDialog();

            if (result != true)
                return false; // User cancelled

            var operation = dialog.GetSelectedOperation();
            if (operation == null)
                return false;

            // Selection-based operations require a current selection of fabrication parts.
            bool isSelectionOp = operation.OperationType == BatchOperationType.ClearSpool
                || operation.OperationType == BatchOperationType.SetSpoolValue
                || operation.OperationType == BatchOperationType.ReportSummary;
            if (isSelectionOp && selectedParts.Count == 0)
            {
                TaskDialog.Show("Batch Operations",
                    "No fabrication parts in selection. Select MEP fabrication parts and retry.");
                return false;
            }

            // Execute operation
            switch (operation.OperationType)
            {
                case BatchOperationType.ClearSpool:
                    return ExecuteClearSpool(doc, selectedParts);
                case BatchOperationType.SetSpoolValue:
                    return ExecuteSetSpool(doc, selectedParts, operation.TargetValue);
                case BatchOperationType.ReportSummary:
                    return ExecuteReportSummary(selectedParts);
                case BatchOperationType.ExportMajBySpool:
                    return ExecuteExportMaj(doc, allParts, operation.SelectedSpools);
                case BatchOperationType.CreatePublishSetBySpool:
                    return ExecuteCreatePublishSet(doc, allParts, operation.SelectedSpools);
                default:
                    return false;
            }
        }

        /// <summary>Group selected spools' parts (preserving spool selection order).</summary>
        private static List<KeyValuePair<string, ICollection<ElementId>>> GroupSelectedSpoolParts(
            List<FabricationPart> allParts, List<string> selectedSpools)
        {
            var lookup = allParts.ToLookup(p => SpoolExportLogic.NormalizeSpool(p.SpoolName));
            var result = new List<KeyValuePair<string, ICollection<ElementId>>>();
            foreach (var spool in selectedSpools)
            {
                var ids = lookup[spool].Select(p => p.Id).ToList();
                if (ids.Count > 0)
                    result.Add(new KeyValuePair<string, ICollection<ElementId>>(spool, ids));
            }
            return result;
        }

        private bool ExecuteExportMaj(Document doc, List<FabricationPart> allParts, List<string> selectedSpools)
        {
            var folderDialog = new OpenFolderDialog { Title = "Choose a folder for the MAJ files" };
            if (folderDialog.ShowDialog() != true)
                return false;
            string folder = folderDialog.FolderName;

            var spoolParts = GroupSelectedSpoolParts(allParts, selectedSpools);
            int filesWritten = 0, partsExported = 0;
            var failures = new List<string>();

            // SaveAsFabricationJob must run inside a transaction (it triggers internal parsing).
            using (var t = new Transaction(doc, "Export Spools to MAJ"))
            {
                t.Start();
                foreach (var pair in spoolParts)
                {
                    string path = Path.Combine(folder, SpoolExportLogic.MajFileName(pair.Key));
                    ISet<ElementId> ids = new HashSet<ElementId>(pair.Value);
                    if (FabricationJobExporter.SaveJob(doc, ids, path))
                    {
                        filesWritten++;
                        partsExported += ids.Count;
                    }
                    else
                    {
                        failures.Add(pair.Key);
                    }
                }
                t.Commit();
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Exported {filesWritten} MAJ file(s) ({partsExported} parts) to:");
            sb.AppendLine(folder);
            if (failures.Count > 0)
                sb.AppendLine($"\nFailed spools: {string.Join(", ", failures)}\n(See log: {Logger.LogFilePath})");
            TaskDialog.Show("Export MAJ", sb.ToString().TrimEnd());
            return failures.Count == 0;
        }

        private bool ExecuteCreatePublishSet(Document doc, List<FabricationPart> allParts, List<string> selectedSpools)
        {
            var spoolParts = GroupSelectedSpoolParts(allParts, selectedSpools);
            if (spoolParts.Count == 0)
            {
                TaskDialog.Show("Publish Set", "No parts found for the selected spools.");
                return false;
            }

            List<View3D> views;
            using (var t = new Transaction(doc, "Create Spool Views"))
            {
                t.Start();
                views = SpoolViewPublisher.CreateIsolatedViews(doc, spoolParts);
                t.Commit();
            }

            if (views.Count == 0)
            {
                TaskDialog.Show("Publish Set", "Could not create any spool views. See log:\n" + Logger.LogFilePath);
                return false;
            }

            // Print/publish-set settings are not transaction-bound; save after the views commit.
            string setName = $"Spools ({views.Count})";
            bool setSaved = SpoolViewPublisher.SaveAsPublishSet(doc, views, setName);

            var sb = new StringBuilder();
            sb.AppendLine($"Created {views.Count} isolated spool view(s).");
            sb.AppendLine(setSaved
                ? $"Publish set saved (View/Sheet Set named like \"{setName}\")."
                : $"Views created, but saving the publish set failed (see log: {Logger.LogFilePath}).");
            TaskDialog.Show("Publish Set", sb.ToString().TrimEnd());
            return setSaved;
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

