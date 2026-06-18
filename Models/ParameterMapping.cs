using System;
using System.Collections.Generic;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Represents a single parameter mapping rule: copy from source param to target param.
    /// </summary>
    public class ParameterMapping
    {
        public string SourceParameter { get; set; }
        public string TargetParameter { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ParameterMapping() { }

        public ParameterMapping(string source, string target)
        {
            SourceParameter = source;
            TargetParameter = target;
            IsEnabled = true;
        }

        public override string ToString() => $"{SourceParameter} → {TargetParameter}";
    }

    /// <summary>
    /// Collection of parameter mappings with JSON persistence.
    /// Stored in %APPDATA%\Autodesk\Revit\Addins\RevitSpoolCopy\mappings.json
    /// </summary>
    public class ParameterMappingCollection
    {
        public List<ParameterMapping> Mappings { get; set; } = new List<ParameterMapping>();
        public DateTime LastModified { get; set; } = DateTime.Now;
    }
}
