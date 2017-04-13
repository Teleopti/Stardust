using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns
{
    public class LineColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        public LineColumn(string bindingProperty) : base(bindingProperty,110)
        {
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (dataItems.Count == 0)
            {
                e.Style.CellValue = PeopleWorksheet.StateHolder.CurrentChildName;
                return;
            }

            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                T dataItem = dataItems[e.RowIndex];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
            }

            if (dataItems.Count > 0 && e.RowIndex == 1)
            {
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.CellValue = _propertyReflector.GetValue(dataItem, BindingProperty);
                e.Style.VerticalAlignment = GridVerticalAlignment.Top;
                e.Style.Font.Bold = true;
            }

            e.Handled = true;
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Handled = true;
        }
    }
}