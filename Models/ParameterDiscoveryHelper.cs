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
            if (parts == null)
                return new List<string>();

            var views = parts.Where(p => p != null).Select(p => (IElementView)new RevitElementView(p));
            return ParameterLogic.CollectParameterNames(views);
        }

        /// <summary>
        /// Read parameter value from an element. Returns null if param doesn't exist or has no value.
        /// </summary>
        public static string ReadParameterValue(Element element, string paramName)
        {
            if (element == null)
                return null;

            return ParameterLogic.ReadValue(new RevitElementView(element), paramName);
        }

        /// <summary>
        /// Write value to a parameter on an element. Returns true on success.
        /// </summary>
        public static bool WriteParameterValue(Element element, string paramName, string value)
        {
            if (element == null)
                return false;

            return ParameterLogic.WriteValue(new RevitElementView(element), paramName, value);
        }
    }
}
