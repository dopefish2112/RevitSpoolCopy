using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Adapts a Revit <see cref="Parameter"/> to <see cref="IParameterView"/>.
    /// Thin pass-through; the decision logic lives in <see cref="ParameterLogic"/>.
    /// </summary>
    internal sealed class RevitParameterView : IParameterView
    {
        private readonly Parameter _p;

        public RevitParameterView(Parameter p) => _p = p;

        public string Name => _p.Definition?.Name;
        public bool HasValue => _p.HasValue;
        public bool IsReadOnly => _p.IsReadOnly;
        public bool IsStringStorage => _p.StorageType == StorageType.String;

        public string AsStringValue() => _p.AsString();
        public string AsDisplayValue() => _p.AsValueString();
        public void SetStringValue(string value) => _p.Set(value);
        public void SetDisplayValue(string value) => _p.SetValueString(value);
    }

    /// <summary>
    /// Adapts a Revit <see cref="Element"/> to <see cref="IElementView"/>.
    /// </summary>
    internal sealed class RevitElementView : IElementView
    {
        private readonly Element _e;

        public RevitElementView(Element e) => _e = e;

        public IEnumerable<IParameterView> AllParameters
        {
            get
            {
                foreach (Parameter p in _e.Parameters)
                {
                    if (p != null)
                        yield return new RevitParameterView(p);
                }
            }
        }

        public IParameterView Lookup(string name)
        {
            Parameter p = _e.LookupParameter(name);
            return p == null ? null : new RevitParameterView(p);
        }

        public IParameterView AssemblyNameParameter
        {
            get
            {
                Parameter p = _e.get_Parameter(BuiltInParameter.ASSEMBLY_NAME);
                return p == null ? null : new RevitParameterView(p);
            }
        }
    }
}
