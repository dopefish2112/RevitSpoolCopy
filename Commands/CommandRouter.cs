using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitSpoolCopy.Commands;

namespace RevitSpoolCopy
{
    /// <summary>
    /// Routes ribbon button clicks to the appropriate ICommand implementation.
    /// This is the single IExternalCommand entry point for all commands.
    /// Commands register themselves via SetActiveCommand() before button click.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CommandRouter : IExternalCommand
    {
        private static ICommand _activeCommand = null;
        private static readonly object _lock = new object();

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

                // Get the active command (set by button activation)
                ICommand cmd = null;
                lock (_lock)
                {
                    cmd = _activeCommand;
                    _activeCommand = null; // Reset for next click
                }

                if (cmd == null)
                {
                    message = "No command registered.";
                    return Result.Failed;
                }

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

        /// <summary>
        /// Register the command to execute on next button click.
        /// </summary>
        public static void SetActiveCommand(ICommand command)
        {
            lock (_lock)
            {
                _activeCommand = command;
            }
        }
    }
}
