using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Loads and saves parameter mappings to JSON config file.
    /// Config location: %APPDATA%\Autodesk\Revit\Addins\RevitSpoolCopy\mappings.json
    /// </summary>
    public static class ParameterMappingConfig
    {
        private static string ConfigDirectory =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Autodesk", "Revit", "Addins", "RevitSpoolCopy");

        private static string ConfigFilePath =>
            Path.Combine(ConfigDirectory, "mappings.json");

        /// <summary>
        /// Load mappings from JSON config file. Returns empty collection if file doesn't exist.
        /// </summary>
        public static ParameterMappingCollection Load()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                    return new ParameterMappingCollection();

                string json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<ParameterMappingCollection>(json);
                return config ?? new ParameterMappingCollection();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading mappings: {ex.Message}");
                return new ParameterMappingCollection();
            }
        }

        /// <summary>
        /// Save mappings to JSON config file. Creates directory if needed.
        /// </summary>
        public static bool Save(ParameterMappingCollection config)
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                    Directory.CreateDirectory(ConfigDirectory);

                config.LastModified = DateTime.Now;
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving mappings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the absolute path to the config file (for debugging/manual inspection).
        /// </summary>
        public static string GetConfigFilePath() => ConfigFilePath;
    }
}
