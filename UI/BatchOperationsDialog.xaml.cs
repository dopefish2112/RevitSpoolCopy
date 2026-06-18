using System.Windows;
using Autodesk.Revit.UI;
using RevitSpoolCopy.Models;

namespace RevitSpoolCopy.UI
{
    public partial class BatchOperationsDialog : Window
    {
        private BatchOperation _selectedOperation;

        public BatchOperationsDialog()
        {
            InitializeComponent();
            ClearSpoolRadio.IsChecked = true;
        }

        private void ClearSpoolRadio_Checked(object sender, RoutedEventArgs e)
        {
            ValueInputPanel.Visibility = Visibility.Collapsed;
        }

        private void SetSpoolRadio_Checked(object sender, RoutedEventArgs e)
        {
            ValueInputPanel.Visibility = Visibility.Visible;
            ValueTextBox.Focus();
        }

        private void ReportRadio_Checked(object sender, RoutedEventArgs e)
        {
            ValueInputPanel.Visibility = Visibility.Collapsed;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClearSpoolRadio.IsChecked == true)
            {
                _selectedOperation = new BatchOperation(BatchOperationType.ClearSpool);
            }
            else if (SetSpoolRadio.IsChecked == true)
            {
                string value = ValueTextBox.Text ?? "";
                if (string.IsNullOrWhiteSpace(value))
                {
                    TaskDialog.Show("Batch Operations", "Please enter a value for the Spool field.");
                    return;
                }
                _selectedOperation = new BatchOperation(BatchOperationType.SetSpoolValue, value);
            }
            else if (ReportRadio.IsChecked == true)
            {
                _selectedOperation = new BatchOperation(BatchOperationType.ReportSummary);
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public BatchOperation GetSelectedOperation() => _selectedOperation;
    }
}
