using System;
using System.Collections.Generic;
using System.Text;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Pure (Revit-free) helpers for the spool-driven export/publish features:
    /// filtering parts to a selected set of spools, building safe .MAJ file names,
    /// and naming generated views. Unit-testable; the Revit commands call into here.
    /// </summary>
    public static class SpoolExportLogic
    {
        /// <summary>Label used for parts whose spool is empty/unset (matches SpoolManager grouping).</summary>
        public const string EmptySpoolLabel = "(empty)";

        // Fixed invalid set (Windows file-name reserved chars) so behavior is deterministic.
        private static readonly char[] InvalidFileNameChars =
            { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

        /// <summary>
        /// Normalize a spool name into a value usable for grouping/selection.
        /// Null/whitespace becomes <see cref="EmptySpoolLabel"/>.
        /// </summary>
        public static string NormalizeSpool(string spoolName) =>
            string.IsNullOrWhiteSpace(spoolName) ? EmptySpoolLabel : spoolName;

        /// <summary>
        /// Return the ids of parts whose (normalized) spool is in <paramref name="selectedSpools"/>,
        /// preserving input order. Each input pair is (rawSpoolName, id).
        /// </summary>
        public static List<T> CollectIdsForSpools<T>(
            IEnumerable<KeyValuePair<string, T>> partSpools, ISet<string> selectedSpools)
        {
            var result = new List<T>();
            if (partSpools == null || selectedSpools == null || selectedSpools.Count == 0)
                return result;

            foreach (var pair in partSpools)
            {
                if (selectedSpools.Contains(NormalizeSpool(pair.Key)))
                    result.Add(pair.Value);
            }
            return result;
        }

        /// <summary>
        /// Sanitize an arbitrary string into a safe file-name stem (no extension).
        /// Invalid chars become '_', result is trimmed; empty input yields "spool".
        /// </summary>
        public static string SanitizeFileStem(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "spool";

            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (c < 32 || Array.IndexOf(InvalidFileNameChars, c) >= 0)
                    sb.Append('_');
                else
                    sb.Append(c);
            }

            // Trim trailing dots/spaces (Windows disallows these at the end of a name).
            string stem = sb.ToString().Trim().TrimEnd('.', ' ');
            return stem.Length == 0 ? "spool" : stem;
        }

        /// <summary>Build a .MAJ file name (stem + ".maj") for a spool.</summary>
        public static string MajFileName(string spoolName) =>
            SanitizeFileStem(NormalizeSpool(spoolName)) + ".maj";

        /// <summary>Build a stable, readable view name for a spool's isolated view.</summary>
        public static string ViewName(string spoolName) =>
            "Spool - " + NormalizeSpool(spoolName);

        /// <summary>
        /// Build a readable name for a single view/publish set covering several spools.
        /// Lists up to three spool names, then "+N more". Empty input yields "Spools".
        /// </summary>
        public static string CombinedViewName(IList<string> spoolNames)
        {
            if (spoolNames == null || spoolNames.Count == 0)
                return "Spools";
            if (spoolNames.Count == 1)
                return "Spool - " + NormalizeSpool(spoolNames[0]);

            const int maxShown = 3;
            var shown = new List<string>();
            for (int i = 0; i < spoolNames.Count && i < maxShown; i++)
                shown.Add(NormalizeSpool(spoolNames[i]));

            string list = string.Join(", ", shown);
            int extra = spoolNames.Count - shown.Count;
            return extra > 0 ? $"Spools - {list} +{extra} more" : $"Spools - {list}";
        }
    }
}
