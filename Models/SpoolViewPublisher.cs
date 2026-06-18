using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Creates a per-spool isolated 3D view (showing only that spool's parts) and gathers
    /// the views into a print/publish set (ViewSheetSet).
    /// </summary>
    public static class SpoolViewPublisher
    {
        /// <summary>
        /// For each spool create a 3D view isolated to its parts. Must run inside an open
        /// transaction. Returns the created views (in spool order). Failures per spool are
        /// logged and skipped.
        /// </summary>
        public static List<View3D> CreateIsolatedViews(
            Document doc, IList<KeyValuePair<string, ICollection<ElementId>>> spoolParts)
        {
            var created = new List<View3D>();
            if (doc == null || spoolParts == null)
                return created;

            ElementId vftId = Get3DViewFamilyTypeId(doc);
            if (vftId == ElementId.InvalidElementId)
            {
                Logger.Warn("CreateIsolatedViews: no 3D ViewFamilyType found.");
                return created;
            }

            var existingNames = new HashSet<string>(
                new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>()
                    .Where(v => !v.IsTemplate).Select(v => v.Name),
                StringComparer.OrdinalIgnoreCase);

            foreach (var pair in spoolParts)
            {
                string spool = pair.Key;
                ICollection<ElementId> ids = pair.Value;
                if (ids == null || ids.Count == 0)
                    continue;

                try
                {
                    var view = View3D.CreateIsometric(doc, vftId);
                    view.Name = UniqueName(SpoolExportLogic.ViewName(spool), existingNames);
                    existingNames.Add(view.Name);

                    // Show only this spool's parts.
                    view.IsolateElementsTemporary(ids);
                    view.ConvertTemporaryHideIsolateToPermanent();

                    created.Add(view);
                }
                catch (Exception ex)
                {
                    Logger.Error($"CreateIsolatedViews(spool '{spool}')", ex);
                }
            }

            return created;
        }

        /// <summary>
        /// Save the given views as a named print/publish set (ViewSheetSet). Should be called
        /// OUTSIDE a transaction (PrintManager settings are not transaction-bound). Returns true
        /// on success.
        /// </summary>
        public static bool SaveAsPublishSet(Document doc, IList<View3D> views, string setName)
        {
            if (doc == null || views == null || views.Count == 0 || string.IsNullOrWhiteSpace(setName))
                return false;

            try
            {
                PrintManager pm = doc.PrintManager;
                pm.PrintRange = PrintRange.Select;
                ViewSheetSetting vss = pm.ViewSheetSetting;

                var viewSet = new ViewSet();
                foreach (var v in views)
                    viewSet.Insert(v);

                vss.CurrentViewSheetSet.Views = viewSet;
                vss.SaveAs(UniquePublishSetName(doc, setName));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"SaveAsPublishSet('{setName}')", ex);
                return false;
            }
        }

        private static ElementId Get3DViewFamilyTypeId(Document doc)
        {
            var vft = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(t => t.ViewFamily == ViewFamily.ThreeDimensional);
            return vft?.Id ?? ElementId.InvalidElementId;
        }

        private static string UniqueName(string baseName, HashSet<string> taken)
        {
            if (!taken.Contains(baseName))
                return baseName;
            for (int i = 2; ; i++)
            {
                string candidate = $"{baseName} ({i})";
                if (!taken.Contains(candidate))
                    return candidate;
            }
        }

        private static string UniquePublishSetName(Document doc, string baseName)
        {
            var taken = new HashSet<string>(
                new FilteredElementCollector(doc).OfClass(typeof(ViewSheetSet))
                    .Cast<ViewSheetSet>().Select(s => s.Name),
                StringComparer.OrdinalIgnoreCase);

            if (!taken.Contains(baseName))
                return baseName;
            for (int i = 2; ; i++)
            {
                string candidate = $"{baseName} ({i})";
                if (!taken.Contains(candidate))
                    return candidate;
            }
        }
    }
}
