using System;
using System.Reflection;
using Autodesk.Revit.UI;
using RevitSpoolCopy.Commands;

namespace RevitSpoolCopy
{
    /// <summary>
    /// IExternalApplication entry point. Builds the ribbon UI and registers commands.
    /// </summary>
    public class App : IExternalApplication
    {
        private const string RibbonTabName = "Spool Tools";
        private const string RibbonPanelName = "Fabrication";

        public Result OnStartup(UIControlledApplication app)
        {
            try
            {
                // Ensure ribbon tab exists
                try { app.CreateRibbonTab(RibbonTabName); }
                catch { /* tab already exists */ }

                // Create or get the panel
                RibbonPanel panel = app.CreateRibbonPanel(RibbonTabName, RibbonPanelName);

                // Register available commands to the ribbon
                // For MVP, all buttons route to CommandRouter which dispatches to CopyAssemblyName
                AddCommandButton(panel, new CopyAssemblyNameCommand());
                AddCommandButton(panel, new MapParametersCommand());
                AddCommandButton(panel, new BatchOperationsCommand());
                AddCommandButton(panel, new SpoolManagerCommand());

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("RevitSpoolCopy Error", $"Failed to load addin:\n{ex.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication app) => Result.Succeeded;

        /// <summary>
        /// Add a command button to the ribbon panel.
        /// Routes all buttons to CommandRouter, which dispatches to the appropriate command.
        /// Special case: SpoolManagerCommand has its own direct routing.
        /// TODO: Enhance to support multiple commands by passing context via button ID
        /// </summary>
        private void AddCommandButton(RibbonPanel panel, ICommand cmd)
        {
            string asmPath = Assembly.GetExecutingAssembly().Location;

            // Special handling for SpoolManager - use its own class
            string commandClass = (cmd is SpoolManagerCommand)
                ? typeof(SpoolManagerCommand).FullName
                : typeof(CommandRouter).FullName;

            var btnData = new PushButtonData(
                cmd.Id,
                cmd.DisplayName,
                asmPath,
                commandClass)
            {
                ToolTip = cmd.ToolTip,
                LongDescription = cmd.LongDescription
            };

            panel.AddItem(btnData);
        }
    }
}

