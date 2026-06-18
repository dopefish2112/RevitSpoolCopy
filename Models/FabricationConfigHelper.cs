using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Fabrication;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Guards around the document's Fabrication Configuration. MAJ export (and any fabrication
    /// part work) requires a loaded configuration; without one the API errors or exports empty.
    /// </summary>
    public static class FabricationConfigHelper
    {
        /// <summary>True if a Fabrication Configuration is loaded in the document.</summary>
        public static bool IsConfigurationLoaded(Document doc)
        {
            if (doc == null) return false;
            try
            {
                FabricationConfiguration config = FabricationConfiguration.GetFabricationConfiguration(doc);
                return config != null;
            }
            catch (Exception ex)
            {
                Logger.Error("FabricationConfigHelper.IsConfigurationLoaded", ex);
                return false;
            }
        }
    }
}
