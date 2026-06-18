using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Helper methods for working with FabricationPart instances.
    /// Encapsulates parameter reading/writing logic.
    /// </summary>
    public static class FabricationPartHelper
    {
        /// <summary>
        /// Get a parameter value by name. Tries builtin parameter first, then custom parameter.
        /// </summary>
        public static string GetParameterValue(Element element, string paramName)
        {
            if (element == null)
                return null;

            return ParameterLogic.GetMappedSourceValue(new RevitElementView(element), paramName);
        }

        /// <summary>
        /// Set the Spool field on a fabrication part.
        /// </summary>
        public static bool SetSpoolName(FabricationPart part, string value)
        {
            if (part == null)
                return false;

            try
            {
                part.SpoolName = value ?? string.Empty;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Filter a collection of elements to only FabricationPart instances.
        /// </summary>
        public static List<FabricationPart> FilterFabricationParts(ICollection<ElementId> elementIds, Document doc)
        {
            var parts = new List<FabricationPart>();
            if (doc == null || elementIds == null)
                return parts;

            foreach (var id in elementIds)
            {
                var element = doc.GetElement(id) as FabricationPart;
                if (element != null)
                    parts.Add(element);
            }
            return parts;
        }
    }
}
