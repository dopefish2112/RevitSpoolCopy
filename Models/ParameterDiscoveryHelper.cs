using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Discovers all parameters available on a set of elements.
    /// Used to populate dropdowns in MapParametersDialog.
    /// </summary>
    public static class ParameterDiscoveryHelper
    {
        /// <summary>
        /// Get all unique parameter names from a collection of fabrication parts.
        /// Returns both builtin and custom parameters, sorted alphabetically.
        /// </summary>
        public static List<string> GetAvailableParameters(List<FabricationPart> parts)
        {
            var paramNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var part in parts)
            {
                if (part == null) continue;

                // Add all builtin parameters
                var builtins = part.Parameters;
                foreach (Parameter p in builtins)
                {
                    if (p != null && !string.IsNullOrWhiteSpace(p.Definition?.Name))
                        paramNames.Add(p.Definition.Name);
                }
            }

            return paramNames.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Read parameter value from an element. Returns null if param doesn't exist or has no value.
        /// </summary>
        public static string ReadParameterValue(Element element, string paramName)
        {
            if (element == null || string.IsNullOrWhiteSpace(paramName))
                return null;

            Parameter p = element.LookupParameter(paramName);
            if (p == null || !p.HasValue)
                return null;

            try
            {
                return p.StorageType == StorageType.String ? p.AsString() : p.AsValueString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Write value to a parameter on an element. Returns true on success.
        /// </summary>
        public static bool WriteParameterValue(Element element, string paramName, string value)
        {
            if (element == null || string.IsNullOrWhiteSpace(paramName))
                return false;

            Parameter p = element.LookupParameter(paramName);
            if (p == null || p.IsReadOnly)
                return false;

            try
            {
                if (p.StorageType == StorageType.String)
                    p.Set(value ?? "");
                else
                    p.SetValueString(value ?? "");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
