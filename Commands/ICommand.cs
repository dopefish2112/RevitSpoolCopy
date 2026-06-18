using Autodesk.Revit.UI;
using System;

namespace RevitSpoolCopy.Commands
{
    /// <summary>
    /// Base interface for all Revit addin commands.
    /// Allows dynamic registration and discovery of commands from the ribbon.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Unique identifier for this command (used in button names, logging).
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Display name shown on ribbon button.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Multi-line tooltip shown on hover (split by \n).
        /// </summary>
        string ToolTip { get; }

        /// <summary>
        /// Longer description for the ribbon button.
        /// </summary>
        string LongDescription { get; }

        /// <summary>
        /// Execute the command. Returns true on success, false on failure.
        /// Use TaskDialog for user feedback.
        /// </summary>
        bool Execute(UIDocument uidoc, string message);
    }
}
