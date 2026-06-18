using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitSpoolCopy
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CopySpoolCommand : IExternalCommand
    {
        // Source param shown in Properties as "Assembly Name".
        private const string SourceParamName = "Assembly Name";

        public Result Execute(ExternalCommandData cmd, ref string message, ElementSet elements)
        {
            UIDocument uidoc = cmd.Application.ActiveUIDocument;
            if (uidoc == null) { message = "No active document."; return Result.Failed; }
            Document doc = uidoc.Document;

            // Pre-selected elements only.
            ICollection<ElementId> ids = uidoc.Selection.GetElementIds();
            if (ids.Count == 0)
            {
                TaskDialog.Show("Copy to Spool", "Select one or more fabrication parts first, then run.");
                return Result.Cancelled;
            }

            List<FabricationPart> parts = ids
                .Select(id => doc.GetElement(id) as FabricationPart)
                .Where(p => p != null)
                .ToList();

            if (parts.Count == 0)
            {
                TaskDialog.Show("Copy to Spool",
                    "No fabrication parts in selection. Select MEP fabrication parts and retry.");
                return Result.Cancelled;
            }

            int copied = 0, skippedNoSource = 0;

            using (var t = new Transaction(doc, "Copy Assembly Name to Spool"))
            {
                t.Start();
                foreach (FabricationPart part in parts)
                {
                    string asmName = GetAssemblyName(part);
                    if (string.IsNullOrWhiteSpace(asmName)) { skippedNoSource++; continue; }

                    part.SpoolName = asmName;   // native Spool field
                    copied++;
                }
                t.Commit();
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Copied to Spool: {copied}");
            if (skippedNoSource > 0) sb.AppendLine($"Skipped (no Assembly Name): {skippedNoSource}");
            TaskDialog.Show("Copy to Spool", sb.ToString().TrimEnd());

            return Result.Succeeded;
        }

        // "Assembly Name" in the Properties palette. Try the built-in (assembly the part
        // belongs to) first, then a same-named custom parameter as fallback.
        private static string GetAssemblyName(Element e)
        {
            Parameter p = e.get_Parameter(BuiltInParameter.ASSEMBLY_NAME);
            if (p == null || !p.HasValue) p = e.LookupParameter(SourceParamName);
            if (p == null || !p.HasValue) return null;
            return p.StorageType == StorageType.String ? p.AsString() : p.AsValueString();
        }
    }
}
