using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Ensures a readable "Spool" shared parameter exists and is bound (as an instance
    /// parameter) to the fabrication-part categories, so the native FabricationPart.SpoolName
    /// value can be mirrored into something the Properties palette / schedules / tags can show.
    ///
    /// The add-in owns its own shared-parameter file so the user doesn't have to create one:
    ///   %APPDATA%\Autodesk\Revit\Addins\RevitSpoolCopy\RevitSpoolCopy_SharedParams.txt
    /// </summary>
    public static class SharedParameterHelper
    {
        private const string GroupName = "RevitSpoolCopy";
        public const string SpoolParamName = "Spool";

        // Stable GUID so the parameter is the same definition across projects/sessions.
        private static readonly Guid SpoolParamGuid = new Guid("7e9d2f10-3a4b-4c5d-9e6f-1a2b3c4d5e6f");

        /// <summary>Fabrication-part categories the Spool parameter is bound to.</summary>
        public static readonly BuiltInCategory[] FabricationCategories =
        {
            BuiltInCategory.OST_FabricationPipework,
            BuiltInCategory.OST_FabricationDuctwork,
            BuiltInCategory.OST_FabricationHangers,
            BuiltInCategory.OST_FabricationContainment,
        };

        private static string SharedParamFilePath =>
            Path.Combine(Logger.LogDirectory, "RevitSpoolCopy_SharedParams.txt");

        /// <summary>
        /// Ensure the "Spool" shared parameter exists and is bound to the fabrication
        /// categories as an instance parameter. Must be called inside an open transaction
        /// (it modifies the document's parameter bindings). Returns true if bound/available.
        /// The user's existing shared-parameter file setting is preserved.
        /// </summary>
        public static bool EnsureSpoolParameterBound(Document doc)
        {
            if (doc == null) return false;

            Application app = doc.Application;
            string originalSharedFile = app.SharedParametersFilename;

            try
            {
                ExternalDefinition def = GetOrCreateDefinition(app);
                if (def == null)
                {
                    Logger.Warn("EnsureSpoolParameterBound: could not get/create Spool definition.");
                    return false;
                }

                CategorySet catSet = app.Create.NewCategorySet();
                foreach (var bic in FabricationCategories)
                {
                    Category c = TryGetCategory(doc, bic);
                    if (c != null && c.AllowsBoundParameters)
                        catSet.Insert(c);
                }

                if (catSet.IsEmpty)
                {
                    Logger.Warn("EnsureSpoolParameterBound: no bindable fabrication categories in this document.");
                    return false;
                }

                BindingMap map = doc.ParameterBindings;
                InstanceBinding binding = app.Create.NewInstanceBinding(catSet);

                bool alreadyBound = map.Contains(def);
                bool ok = alreadyBound
                    ? map.ReInsert(def, binding, GroupTypeId.Text)
                    : map.Insert(def, binding, GroupTypeId.Text);

                if (!ok)
                    Logger.Warn($"EnsureSpoolParameterBound: binding {(alreadyBound ? "ReInsert" : "Insert")} returned false.");

                return ok;
            }
            catch (Exception ex)
            {
                Logger.Error("EnsureSpoolParameterBound", ex);
                return false;
            }
            finally
            {
                // Restore whatever shared-parameter file the user had configured.
                if (!string.IsNullOrEmpty(originalSharedFile))
                {
                    try { app.SharedParametersFilename = originalSharedFile; } catch { }
                }
            }
        }

        private static ExternalDefinition GetOrCreateDefinition(Application app)
        {
            EnsureSharedParamFileExists();

            app.SharedParametersFilename = SharedParamFilePath;
            DefinitionFile file = app.OpenSharedParameterFile();
            if (file == null)
                return null;

            DefinitionGroup group = file.Groups.get_Item(GroupName) ?? file.Groups.Create(GroupName);

            if (group.Definitions.get_Item(SpoolParamName) is ExternalDefinition existing)
                return existing;

            var opts = new ExternalDefinitionCreationOptions(SpoolParamName, SpecTypeId.String.Text)
            {
                GUID = SpoolParamGuid,
                Visible = true,
                Description = "Mirror of the native fabrication Spool name (written by RevitSpoolCopy)."
            };
            return group.Definitions.Create(opts) as ExternalDefinition;
        }

        private static void EnsureSharedParamFileExists()
        {
            string path = SharedParamFilePath;
            if (File.Exists(path))
                return;

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Minimal valid (empty) Revit shared-parameter file. Tabs are required.
            const string header =
                "# This is a Revit shared parameter file.\n" +
                "# Do not edit manually.\n" +
                "*META\tVERSION\tMINVERSION\n" +
                "META\t2\t1\n" +
                "*GROUP\tID\tNAME\n" +
                "*PARAM\tGUID\tNAME\tDATATYPE\tDATACATEGORY\tGROUP\tVISIBLE\tDESCRIPTION\tUSERMODIFIABLE\tHIDEWHENNOVALUE\n";
            File.WriteAllText(path, header.Replace("\n", Environment.NewLine));
        }

        private static Category TryGetCategory(Document doc, BuiltInCategory bic)
        {
            try { return Category.GetCategory(doc, bic); }
            catch { return null; }
        }
    }
}
