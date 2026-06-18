using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitSpoolCopy.Models;

namespace RevitSpoolCopy.Commands
{
    /// <summary>
    /// Command: Copy "Assembly Name" parameter to Spool field on selected fabrication parts.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CopyAssemblyNameCommand : ICommand, IExternalCommand
    {
        public string Id => "CopyAssemblyNameToSpool";
        public string DisplayName => "Assembly Name\n-> Spool";
        public string ToolTip => "For each selected fabrication part, copy its 'Assembly Name' value into the Spool field.";
        public string LongDescription => "Select fabrication parts first, then click. Reads the \"Assembly Name\" parameter and writes it to the part's Spool.";

        private const string SourceParamName = "Assembly Name";

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

            // Pre-selected elements only
            var elementIds = uidoc.Selection.GetElementIds();
            if (elementIds.Count == 0)
            {
                TaskDialog.Show("Copy to Spool", "Select one or more fabrication parts first, then run.");
                return false;
            }

            var parts = FabricationPartHelper.FilterFabricationParts(elementIds, doc);
            if (parts.Count == 0)
            {
                TaskDialog.Show("Copy to Spool",
                    "No fabrication parts in selection. Select MEP fabrication parts and retry.");
                return false;
            }

            int copied = 0, skippedNoSource = 0, mirrored = 0;

            using (var t = new Transaction(doc, "Copy Assembly Name to Spool"))
            {
                t.Start();

                // Ensure the readable "Spool" shared parameter exists & is bound, so the
                // value is also visible in Properties / schedules / tags.
                bool spoolParamBound = SharedParameterHelper.EnsureSpoolParameterBound(doc);

                foreach (var part in parts)
                {
                    string asmName = FabricationPartHelper.GetParameterValue(part, SourceParamName);
                    if (string.IsNullOrWhiteSpace(asmName))
                    {
                        skippedNoSource++;
                        continue;
                    }

                    if (FabricationPartHelper.SetSpoolName(part, asmName))
                        copied++;
                    else
                        skippedNoSource++;

                    // Mirror into the readable shared parameter.
                    if (spoolParamBound &&
                        ParameterDiscoveryHelper.WriteParameterValue(part, SharedParameterHelper.SpoolParamName, asmName))
                        mirrored++;
                }
                t.Commit();
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Copied to Spool: {copied}");
            sb.AppendLine($"Also written to readable \"{SharedParameterHelper.SpoolParamName}\" parameter: {mirrored}");
            if (skippedNoSource > 0) sb.AppendLine($"Skipped (no Assembly Name or error): {skippedNoSource}");
            TaskDialog.Show("Copy to Spool", sb.ToString().TrimEnd());

            return true;
        }
    }
}
