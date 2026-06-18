using System;
using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitSpoolCopy.Commands;

namespace RevitSpoolCopy.UI
{
    public partial class SpoolManagerDialog : Window
    {
        private List<FabricationPart> _allParts;
        private List<SpoolInfo> _spoolInfos;
        private SpoolAction _selectedAction;

        public SpoolManagerDialog(List<FabricationPart> allParts, List<SpoolInfo> spoolInfos)
        {
            InitializeComponent();
            _allParts = allParts;
            _spoolInfos = spoolInfos;

            SpoolDataGrid.ItemsSource = spoolInfos;
            RenameRadio.IsChecked = true;
        }

        private void RenameRadio_Checked(object sender, RoutedEventArgs e)
        {
            RenameTextBox.IsEnabled = true;
            RenameTextBox.Focus();
        }

        private void DeleteRadio_Checked(object sender, RoutedEventArgs e)
        {
            RenameTextBox.IsEnabled = false;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = SpoolDataGrid.SelectedItem as SpoolInfo;
            if (selected == null)
            {
                TaskDialog.Show("Spool Manager", "Please select a spool from the list.");
                return;
            }

            if (RenameRadio.IsChecked == true)
            {
                string newName = RenameTextBox.Text ?? "";
                if (string.IsNullOrWhiteSpace(newName))
                {
                    TaskDialog.Show("Spool Manager", "Please enter a new name for the spool.");
                    return;
                }

                if (newName == selected.SpoolName)
                {
                    TaskDialog.Show("Spool Manager", "New name must be different from current name.");
                    return;
                }

                _selectedAction = new SpoolAction(SpoolActionType.RenameSpool, selected.SpoolName, newName);
            }
            else if (DeleteRadio.IsChecked == true)
            {
                _selectedAction = new SpoolAction(SpoolActionType.DeleteSpool, selected.SpoolName);
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public SpoolAction GetSelectedAction() => _selectedAction;
    }
}
