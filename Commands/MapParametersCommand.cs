using Autodesk.Revit.UI;

namespace RevitSpoolCopy.Commands
{
    /// <summary>
    /// Placeholder for "Map Parameters" command.
    /// Maps custom parameters from one part property to another.
    /// </summary>
    public class MapParametersCommand : ICommand
    {
        public string Id => "MapParameters";
        public string DisplayName => "Map\nParameters";
        public string ToolTip => "[WIP] Map one parameter to another on selected parts.";
        public string LongDescription => "Select source and target parameters, apply mapping to selected fabrication parts.";

        public bool Execute(UIDocument uidoc, string message)
        {
            TaskDialog.Show("Map Parameters", "This feature is coming soon.");
            return false;
        }
    }
}
