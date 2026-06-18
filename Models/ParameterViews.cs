using System.Collections.Generic;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Revit-free view of a single parameter. Lets the read/write decision logic in
    /// <see cref="ParameterLogic"/> be unit-tested without a Revit runtime.
    /// The production implementation (RevitParameterView) wraps Autodesk.Revit.DB.Parameter.
    /// </summary>
    public interface IParameterView
    {
        string Name { get; }
        bool HasValue { get; }
        bool IsReadOnly { get; }

        /// <summary>True when the underlying storage type is String.</summary>
        bool IsStringStorage { get; }

        /// <summary>Read as a raw string (Parameter.AsString).</summary>
        string AsStringValue();

        /// <summary>Read as a formatted display string (Parameter.AsValueString).</summary>
        string AsDisplayValue();

        /// <summary>Write a raw string value (Parameter.Set).</summary>
        void SetStringValue(string value);

        /// <summary>Write via a formatted display string (Parameter.SetValueString).</summary>
        void SetDisplayValue(string value);
    }

    /// <summary>
    /// Revit-free view of an element's parameter surface.
    /// The production implementation (RevitElementView) wraps Autodesk.Revit.DB.Element.
    /// </summary>
    public interface IElementView
    {
        /// <summary>All parameters on the element (builtin + custom).</summary>
        IEnumerable<IParameterView> AllParameters { get; }

        /// <summary>Look up a parameter by name, or null if none.</summary>
        IParameterView Lookup(string name);

        /// <summary>The builtin Assembly Name parameter, or null if not applicable.</summary>
        IParameterView AssemblyNameParameter { get; }
    }
}
