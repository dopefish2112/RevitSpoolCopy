using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Fabrication;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Exports MEP fabrication parts to an Autodesk Fabrication job (.MAJ) file via the
    /// public Revit API (FabricationPart.SaveAsFabricationJob).
    ///
    /// Requirements (per Revit API): a Fabrication Configuration must be loaded in the
    /// project, and the call must run inside an open transaction (it triggers internal
    /// element parsing even though it writes to an external file).
    /// </summary>
    public static class FabricationJobExporter
    {
        /// <summary>
        /// Save the given fabrication part ids to <paramref name="filePath"/> (.MAJ).
        /// Caller must have an open transaction. Returns true on success.
        /// </summary>
        public static bool SaveJob(Document doc, ISet<ElementId> partIds, string filePath)
        {
            if (doc == null || partIds == null || partIds.Count == 0 || string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                // Single bool ctor: include related ancillaries/supports for the parts.
                var options = new FabricationSaveJobOptions(true);
                FabricationPart.SaveAsFabricationJob(doc, partIds, filePath, options);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"FabricationJobExporter.SaveJob('{filePath}')", ex);
                return false;
            }
        }
    }
}
