using System.Collections.ObjectModel;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
    public class ReadOnlyTextColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _groupHeaderText;
        private bool _makeStatic;
        
        public ReadOnlyTextColumn(string bindingProperty, string headerText, int preferredWidth = 110, bool makeStatic = false) : base(bindingProperty,preferredWidth)
        {
            _headerText = headerText;
            _makeStatic = makeStatic;
        }

        public ReadOnlyTextColumn(string bindingProperty, string headerText, string groupHeaderText, int preferredWidth = 110, bool makeStatic = false)
            : this(bindingProperty, headerText, preferredWidth, makeStatic)
        {
            _groupHeaderText = groupHeaderText;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (string.IsNullOrEmpty(_groupHeaderText))
            {
                SetUpSingleHeader(e, dataItems);
            }
            else
            {
                SetUpMultipleHeaders(e, dataItems);
            }
        }

        private void SetUpSingleHeader(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;
            }
            else
            {
                if (dataItems.Count == 0 || dataItems.Count <= (e.RowIndex - 1)) return;
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);

                e.Style.ReadOnly = true;
                e.Style.CellType = (_makeStatic) ? "Static" : e.Style.CellType;
                e.Style.TextColor = Color.DimGray;

                OnCellDisplayChanged(dataItem, e);

                e.Handled = true;
            }
        }

        private void SetUpMultipleHeaders(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0)
            {
                e.Style.CellValue = _groupHeaderText;
                e.Style.Clickable = false;
            }
            else if (e.RowIndex == 1 && e.ColIndex > 0)
            {
                e.Style.CellValue = _headerText;

            }
            else
            {
                if (dataItems.Count == 0) return;
                T dataItem = dataItems[e.RowIndex - 2];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);

                e.Style.ReadOnly = true;
                e.Style.CellType = (_makeStatic) ? "Static" : e.Style.CellType;
                e.Style.TextColor = Color.DimGray;
                OnCellDisplayChanged(dataItem, e);
                e.Handled = true;
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Handled = true;
        }
    }
}