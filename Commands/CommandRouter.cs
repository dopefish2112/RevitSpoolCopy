using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitSpoolCopy.Commands;

namespace RevitSpoolCopy
{
    /// <summary>
    /// Routes ribbon button clicks to the appropriate ICommand implementation.
    /// This is the single IExternalCommand entry point.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CommandRouter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                UIDocument uidoc = uiApp.ActiveUIDocument;

                if (uidoc == null)
                {
                    message = "No active document.";
                    return Result.Failed;
                }

                // Route to the appropriate command. For MVP, always route to CopyAssemblyName.
                // TODO: Implement button tag or context-based routing for multiple commands

                var cmd = new CopyAssemblyNameCommand();
                string msg = "";
                bool success = cmd.Execute(uidoc, msg);
                message = msg;

                return success ? Result.Succeeded : Result.Failed;
            }
            catch (Exception ex)
            {
                message = $"Error: {ex.Message}";
                return Result.Failed;
            }
        }
    }
}
