using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Pure (Revit-free) parameter read/write/discovery logic operating over the
    /// <see cref="IElementView"/> / <see cref="IParameterView"/> seam. The Revit helpers
    /// (FabricationPartHelper, ParameterDiscoveryHelper) are thin adapters that delegate here.
    /// This class is fully unit-testable with fakes.
    /// </summary>
    public static class ParameterLogic
    {
        /// <summary>
        /// Collect all unique, non-blank parameter names across the elements,
        /// case-insensitive, sorted alphabetically.
        /// </summary>
        public static List<string> CollectParameterNames(IEnumerable<IElementView> elements)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (elements == null)
                return names.ToList();

            foreach (var element in elements)
            {
                if (element == null) continue;
                foreach (var p in element.AllParameters)
                {
                    if (p != null && !string.IsNullOrWhiteSpace(p.Name))
                        names.Add(p.Name);
                }
            }

            return names.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Read a parameter value by name. Returns null if the element/name is missing,
        /// the parameter doesn't exist, has no value, or reading throws.
        /// </summary>
        public static string ReadValue(IElementView element, string paramName)
        {
            if (element == null || string.IsNullOrWhiteSpace(paramName))
                return null;

            var p = element.Lookup(paramName);
            return ReadIfHasValue(p);
        }

        /// <summary>
        /// Write a value to a parameter by name. Returns false if the element/name is missing,
        /// the parameter doesn't exist, is read-only, or writing throws.
        /// </summary>
        public static bool WriteValue(IElementView element, string paramName, string value)
        {
            if (element == null || string.IsNullOrWhiteSpace(paramName))
                return false;

            var p = element.Lookup(paramName);
            if (p == null || p.IsReadOnly)
                return false;

            try
            {
                if (p.IsStringStorage)
                    p.SetStringValue(value ?? "");
                else
                    p.SetDisplayValue(value ?? "");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Resolve a source value for mapping. "Assembly Name" reads the builtin
        /// Assembly Name parameter first, then falls back to a same-named custom parameter.
        /// Any other name reads the custom parameter directly. Returns null when absent.
        /// </summary>
        public static string GetMappedSourceValue(IElementView element, string paramName)
        {
            if (element == null || string.IsNullOrWhiteSpace(paramName))
                return null;

            if (paramName.Equals("Assembly Name", StringComparison.OrdinalIgnoreCase))
            {
                var builtin = ReadIfHasValue(element.AssemblyNameParameter);
                if (builtin != null)
                    return builtin;
            }

            return ReadIfHasValue(element.Lookup(paramName));
        }

        private static string ReadIfHasValue(IParameterView p)
        {
            if (p == null || !p.HasValue)
                return null;

            try
            {
                return p.IsStringStorage ? p.AsStringValue() : p.AsDisplayValue();
            }
            catch
            {
                return null;
            }
        }
    }
}
