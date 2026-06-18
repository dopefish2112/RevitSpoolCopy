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
        /// Create a single 3D view isolated to all the given parts (the union of the selected
        /// spools). Must run inside an open transaction. Returns the view, or null on failure.
        /// </summary>
        public static View3D CreateIsolatedView(
            Document doc, ICollection<ElementId> partIds, string viewName)
        {
            if (doc == null || partIds == null || partIds.Count == 0)
                return null;

            ElementId vftId = Get3DViewFamilyTypeId(doc);
            if (vftId == ElementId.InvalidElementId)
            {
                Logger.Warn("CreateIsolatedView: no 3D ViewFamilyType found.");
                return null;
            }

            var existingNames = new HashSet<string>(
                new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>()
                    .Where(v => !v.IsTemplate).Select(v => v.Name),
                StringComparer.OrdinalIgnoreCase);

            try
            {
                var view = View3D.CreateIsometric(doc, vftId);
                view.Name = UniqueName(string.IsNullOrWhiteSpace(viewName) ? "Spools" : viewName, existingNames);

                // Show only the selected spools' parts.
                view.IsolateElementsTemporary(partIds);
                view.ConvertTemporaryHideIsolateToPermanent();

                return view;
            }
            catch (Exception ex)
            {
                Logger.Error($"CreateIsolatedView('{viewName}')", ex);
                return null;
            }
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
