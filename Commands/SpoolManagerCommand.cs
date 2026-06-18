using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitSpoolCopy.Models;
using RevitSpoolCopy.UI;

namespace RevitSpoolCopy.Commands
{
    /// <summary>
    /// Command: Manage all spools in the current model.
    /// List, rename, or delete spools across all fabrication parts.
    /// Implements both ICommand and IExternalCommand for direct ribbon invocation.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SpoolManagerCommand : ICommand, IExternalCommand
    {
        public string Id => "SpoolManager";
        public string DisplayName => "Spool\nManager";
        public string ToolTip => "List, rename, or delete all spools in the model.";
        public string LongDescription => "View all spools, their part counts, and manage them (rename/delete across all parts).";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                UIDocument uidoc = uiApp.ActiveUIDocument;
                string msg = "";
                bool success = Execute(uidoc, msg);
                message = msg;
                return success ? Result.Succeeded : Result.Failed;
            }
            catch (Exception ex)
            {
                message = $"Error: {ex.Message}";
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

            // Collect all fabrication parts in model
            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(FabricationPart));

            var allParts = collector
                .Cast<Element>()
                .OfType<FabricationPart>()
                .ToList();

            if (allParts.Count == 0)
            {
                TaskDialog.Show("Spool Manager", "No fabrication parts found in this model.");
                return false;
            }

            // Group by spool name
            var spoolGroups = allParts
                .GroupBy(p => p.SpoolName ?? "(empty)")
                .OrderBy(g => g.Key)
                .Select(g => new SpoolInfo { SpoolName = g.Key, PartCount = g.Count() })
                .ToList();

            // Show dialog
            var dialog = new SpoolManagerDialog(allParts, spoolGroups);
            bool? result = dialog.ShowDialog();

            if (result != true)
                return false; // User cancelled

            var action = dialog.GetSelectedAction();
            if (action == null)
                return false;

            // Execute action
            switch (action.ActionType)
            {
                case SpoolActionType.RenameSpool:
                    return ExecuteRenameSpool(doc, allParts, action.SourceSpool, action.TargetValue);
                case SpoolActionType.DeleteSpool:
                    return ExecuteDeleteSpool(doc, allParts, action.SourceSpool);
                default:
                    return false;
            }
        }

        private bool ExecuteRenameSpool(Document doc, List<FabricationPart> allParts, string oldName, string newName)
        {
            int renamed = 0;

            using (var t = new Transaction(doc, "Rename Spool"))
            {
                t.Start();
                foreach (var part in allParts.Where(p => (p.SpoolName ?? "(empty)") == oldName))
                {
                    try
                    {
                        part.SpoolName = newName;
                        renamed++;
                    }
                    catch { }
                }
                t.Commit();
            }

            TaskDialog.Show("Rename Spool", $"Renamed '{oldName}' to '{newName}' on {renamed} parts.");
            return true;
        }

        private bool ExecuteDeleteSpool(Document doc, List<FabricationPart> allParts, string spoolName)
        {
            int cleared = 0;

            using (var t = new Transaction(doc, "Delete Spool"))
            {
                t.Start();
                foreach (var part in allParts.Where(p => (p.SpoolName ?? "(empty)") == spoolName))
                {
                    try
                    {
                        part.SpoolName = "";
                        cleared++;
                    }
                    catch { }
                }
                t.Commit();
            }

            TaskDialog.Show("Delete Spool", $"Cleared spool '{spoolName}' from {cleared} parts.");
            return true;
        }
    }

    /// <summary>
    /// Information about a spool and its parts.
    /// </summary>
    public class SpoolInfo
    {
        public string SpoolName { get; set; }
        public int PartCount { get; set; }
    }

    /// <summary>
    /// Action to perform on a spool.
    /// </summary>
    public enum SpoolActionType
    {
        RenameSpool,
        DeleteSpool
    }

    /// <summary>
    /// Configuration for spool action.
    /// </summary>
    public class SpoolAction
    {
        public SpoolActionType ActionType { get; set; }
        public string SourceSpool { get; set; }
        public string TargetValue { get; set; } = "";

        public SpoolAction() { }

        public SpoolAction(SpoolActionType type, string source, string target = "")
        {
            ActionType = type;
            SourceSpool = source;
            TargetValue = target;
        }
    }
}
