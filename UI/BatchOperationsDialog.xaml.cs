using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.UI;
using RevitSpoolCopy.Commands;
using RevitSpoolCopy.Models;

namespace RevitSpoolCopy.UI
{
    public partial class BatchOperationsDialog : Window
    {
        private BatchOperation _selectedOperation;
        private readonly List<SpoolCheckItem> _spoolItems = new List<SpoolCheckItem>();

        public BatchOperationsDialog() : this(null) { }

        /// <summary>
        /// <paramref name="spools"/> supplies the model's spools (name + part count) for the
        /// spool-based operations. May be null/empty (those operations then have nothing to pick).
        /// </summary>
        public BatchOperationsDialog(IEnumerable<SpoolInfo> spools)
        {
            InitializeComponent();

            if (spools != null)
            {
                foreach (var s in spools)
                    _spoolItems.Add(new SpoolCheckItem
                    {
                        SpoolName = s.SpoolName,
                        Display = $"{s.SpoolName}  ({s.PartCount})"
                    });
            }
            SpoolListBox.ItemsSource = _spoolItems;

            ClearSpoolRadio.IsChecked = true;
        }

        private void Operation_Checked(object sender, RoutedEventArgs e)
        {
            bool isSetSpool = SetSpoolRadio.IsChecked == true;
            bool isSpoolOp = ExportMajRadio.IsChecked == true || PublishSetRadio.IsChecked == true;

            // ValueInputPanel / SpoolSelectPanel may be null during initial radio init.
            if (ValueInputPanel != null)
                ValueInputPanel.Visibility = isSetSpool ? Visibility.Visible : Visibility.Collapsed;
            if (SpoolSelectPanel != null)
                SpoolSelectPanel.Visibility = isSpoolOp ? Visibility.Visible : Visibility.Collapsed;

            if (isSetSpool)
                ValueTextBox?.Focus();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e) => SetAll(true);
        private void SelectNone_Click(object sender, RoutedEventArgs e) => SetAll(false);

        private void SetAll(bool value)
        {
            foreach (var item in _spoolItems)
                item.IsSelected = value;
            SpoolListBox.Items.Refresh();
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
            else if (ExportMajRadio.IsChecked == true || PublishSetRadio.IsChecked == true)
            {
                var chosen = _spoolItems.Where(i => i.IsSelected).Select(i => i.SpoolName).ToList();
                if (chosen.Count == 0)
                {
                    TaskDialog.Show("Batch Operations", "Select at least one spool.");
                    return;
                }
                var type = ExportMajRadio.IsChecked == true
                    ? BatchOperationType.ExportMajBySpool
                    : BatchOperationType.CreatePublishSetBySpool;
                _selectedOperation = new BatchOperation(type) { SelectedSpools = chosen };
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

    /// <summary>Checkable row in the spool list.</summary>
    public class SpoolCheckItem
    {
        public string SpoolName { get; set; }
        public string Display { get; set; }
        public bool IsSelected { get; set; }
    }
}
