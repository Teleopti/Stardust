using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class VisualizeGridColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private readonly string _headerText;
        private readonly string _bindingProperty;
        private readonly string _groupHeaderText;

        public VisualizeGridColumn(string bindingProperty, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
        }

        public VisualizeGridColumn(string bindingProperty, string headerText, string groupHeaderText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _groupHeaderText = groupHeaderText;
        }

        public override int PreferredWidth
        {
            get { return 15; }
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                SetUpSingleHeader(e);
            }
            else
            {
                SetUpMultipleHeaders(e, dataItems);
            }
        }

        #region Set Up Headers

        private void SetUpSingleHeader(GridQueryCellInfoEventArgs e)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
                e.Style.Clickable = true;
            }
            else
            {
                e.Style.ReadOnly = true;
                e.Style.CellType = "Static";
                e.Style.Borders.All = GridBorder.Empty;
                e.Handled = true;
            }
        }

        private void SetUpMultipleHeaders(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0)
            {
                e.Style.CellValue = _groupHeaderText;
            }
            else if (e.RowIndex == 1 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else
            {
                if (dataItems.Count == 0) return;
                T dataItem = dataItems[e.RowIndex - 2];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, _bindingProperty);

                e.Style.ReadOnly = true;
                e.Style.CellType = "Static";
                e.Handled = true;
            }
        }

        #endregion

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Handled = true;
        }
    }
}
