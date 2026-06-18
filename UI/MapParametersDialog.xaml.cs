using System;
using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitSpoolCopy.Models;

namespace RevitSpoolCopy.UI
{
    public partial class MapParametersDialog : Window
    {
        private List<FabricationPart> _parts;
        private List<string> _availableParameters;
        private ParameterMapping _selectedMapping;

        public MapParametersDialog(List<FabricationPart> parts)
        {
            InitializeComponent();
            _parts = parts ?? new List<FabricationPart>();
            _availableParameters = new List<string>();
            DiscoverParameters();
            PopulateDropdowns();
        }

        private void DiscoverParameters()
        {
            _availableParameters = ParameterDiscoveryHelper.GetAvailableParameters(_parts);
        }

        private void PopulateDropdowns()
        {
            SourceParameterCombo.ItemsSource = _availableParameters;
            TargetParameterCombo.ItemsSource = _availableParameters;

            if (_availableParameters.Count > 0)
            {
                SourceParameterCombo.SelectedIndex = 0;
                TargetParameterCombo.SelectedIndex = Math.Min(1, _availableParameters.Count - 1);
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string source = SourceParameterCombo.SelectedItem as string;
            string target = TargetParameterCombo.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
            {
                TaskDialog.Show("Map Parameters", "Please select both source and target parameters.");
                return;
            }

            if (source == target)
            {
                TaskDialog.Show("Map Parameters", "Source and target parameters must be different.");
                return;
            }

            _selectedMapping = new ParameterMapping(source, target);
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public ParameterMapping GetSelectedMapping() => _selectedMapping;
    }
}
