using System;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace RevitSpoolCopy
{
    // Builds the ribbon button. The button IS the interface.
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            const string tab = "Spool Tools";
            try { app.CreateRibbonTab(tab); } catch { /* tab already exists */ }

            RibbonPanel panel = app.CreateRibbonPanel(tab, "Fabrication");

            string asmPath = Assembly.GetExecutingAssembly().Location;

            var btn = new PushButtonData(
                "CopyAsmToSpool",
                "Assembly Name\n-> Spool",
                asmPath,
                typeof(CopySpoolCommand).FullName)
            {
                ToolTip = "For each selected fabrication part, copy its 'Assembly Name' value into the Spool field.",
                LongDescription = "Select fabrication parts first, then click. Reads the \"Assembly Name\" parameter and writes it to the part's Spool."
            };

            panel.AddItem(btn);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication app) => Result.Succeeded;
    }
}
