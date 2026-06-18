using System;
using System.Collections.Generic;
using RevitSpoolCopy.Models;

namespace RevitSpoolCopy.Tests.Models
{
    /// <summary>
    /// In-memory fake parameter for exercising ParameterLogic without Revit.
    /// </summary>
    internal sealed class FakeParameter : IParameterView
    {
        public string Name { get; set; }
        public bool HasValue { get; set; } = true;
        public bool IsReadOnly { get; set; }
        public bool IsStringStorage { get; set; } = true;

        public string StringValue { get; set; }
        public string DisplayValue { get; set; }

        /// <summary>When true, every accessor/mutator throws (simulates a Revit failure).</summary>
        public bool Throws { get; set; }

        public string AsStringValue() => Throws ? throw new InvalidOperationException("boom") : StringValue;
        public string AsDisplayValue() => Throws ? throw new InvalidOperationException("boom") : DisplayValue;

        public void SetStringValue(string value)
        {
            if (Throws) throw new InvalidOperationException("boom");
            StringValue = value;
        }

        public void SetDisplayValue(string value)
        {
            if (Throws) throw new InvalidOperationException("boom");
            DisplayValue = value;
        }
    }

    /// <summary>
    /// In-memory fake element: a bag of named parameters plus an optional builtin
    /// Assembly Name parameter.
    /// </summary>
    internal sealed class FakeElement : IElementView
    {
        private readonly List<IParameterView> _params = new();

        public IParameterView AssemblyNameParameter { get; set; }

        public IEnumerable<IParameterView> AllParameters => _params;

        public FakeElement Add(IParameterView p)
        {
            _params.Add(p);
            return this;
        }

        public IParameterView Lookup(string name)
        {
            foreach (var p in _params)
            {
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                    return p;
            }
            return null;
        }
    }
}
