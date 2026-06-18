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
            if (element == null || string.IsNullOrWhiteSpace(paramName))
                return null;

            // Try builtin ASSEMBLY_NAME if that's what we're looking for
            if (paramName.Equals("Assembly Name", StringComparison.OrdinalIgnoreCase))
            {
                Parameter p = element.get_Parameter(BuiltInParameter.ASSEMBLY_NAME);
                if (p != null && p.HasValue)
                    return p.StorageType == StorageType.String ? p.AsString() : p.AsValueString();
            }

            // Fall back to custom parameter with same name
            Parameter custom = element.LookupParameter(paramName);
            if (custom != null && custom.HasValue)
                return custom.StorageType == StorageType.String ? custom.AsString() : custom.AsValueString();

            return null;
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
