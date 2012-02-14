using System.Collections.ObjectModel;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common.Controls.Columns;
using Teleopti.Ccc.AgentPortal.Helper;

namespace Teleopti.Ccc.AgentPortal.Reports.Grid
{
    internal class MyScheduleGridColumn<T> : ColumnBase<T>
    {
        private static int _preferredWidth = 15;

        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        private string _headerText;
        private string _bindingProperty;
        private string _groupHeaderText;

        public MyScheduleGridColumn(string bindingProperty, string headerText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
        }

        public MyScheduleGridColumn(string bindingProperty, string headerText, string groupHeaderText)
        {
            _headerText = headerText;
            _bindingProperty = bindingProperty;
            _groupHeaderText = groupHeaderText;
        }

        public override int PreferredWidth
        {
            get { return _preferredWidth; }
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
                e.Style.CellType = "Static"; //"(this._makeStatic) ? "Static" : e.Style.CellType;
                e.Style.TextColor = Color.DimGray;
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
                e.Style.TextColor = Color.DimGray;
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