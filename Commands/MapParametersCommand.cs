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
    /// Command: Map a source parameter to a target parameter on selected fabrication parts.
    /// Saves the mapping to config file for reuse in future sessions.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MapParametersCommand : ICommand
    {
        public string Id => "MapParameters";
        public string DisplayName => "Map\nParameters";
        public string ToolTip => "Map one parameter to another on selected fabrication parts.";
        public string LongDescription => "Select source and target parameters, apply mapping to selected fabrication parts. Mapping is saved for future use.";

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
                TaskDialog.Show("Map Parameters", "Select one or more fabrication parts first.");
                return false;
            }

            var parts = FabricationPartHelper.FilterFabricationParts(elementIds, doc);
            if (parts.Count == 0)
            {
                TaskDialog.Show("Map Parameters",
                    "No fabrication parts in selection. Select MEP fabrication parts and retry.");
                return false;
            }

            // Show dialog to select mapping
            var dialog = new MapParametersDialog(parts);
            bool? result = dialog.ShowDialog();

            if (result != true)
                return false; // User cancelled

            var mapping = dialog.GetSelectedMapping();
            if (mapping == null)
                return false;

            // Apply mapping to selected parts
            int mapped = 0, skipped = 0;

            using (var t = new Transaction(doc, "Map Parameters"))
            {
                t.Start();
                foreach (var part in parts)
                {
                    string value = ParameterDiscoveryHelper.ReadParameterValue(part, mapping.SourceParameter);
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        skipped++;
                        continue;
                    }

                    if (ParameterDiscoveryHelper.WriteParameterValue(part, mapping.TargetParameter, value))
                        mapped++;
                    else
                        skipped++;
                }
                t.Commit();
            }

            // Save mapping to config
            var config = ParameterMappingConfig.Load();
            if (!config.Mappings.Any(m => m.SourceParameter == mapping.SourceParameter && m.TargetParameter == mapping.TargetParameter))
            {
                config.Mappings.Add(mapping);
                ParameterMappingConfig.Save(config);
            }

            // Report results
            var sb = new StringBuilder();
            sb.AppendLine($"Mapped '{mapping.SourceParameter}' → '{mapping.TargetParameter}'");
            sb.AppendLine($"Applied to: {mapped} parts");
            if (skipped > 0) sb.AppendLine($"Skipped: {skipped} parts (no source value or write error)");
            TaskDialog.Show("Map Parameters", sb.ToString().TrimEnd());

            return true;
        }
    }
}

