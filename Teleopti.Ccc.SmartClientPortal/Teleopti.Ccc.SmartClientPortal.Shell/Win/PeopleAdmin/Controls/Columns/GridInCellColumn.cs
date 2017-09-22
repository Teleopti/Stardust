using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls.Columns
{
    public class GridInCellColumn<T> : ColumnBase<T>
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();

        public GridInCellColumn(string bindingProperty) : base(bindingProperty, 0)
        {
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.RowIndex == 0 && e.ColIndex > 0)
            {
                e.Style.CellValue = "-";
                e.Handled = true;
            }
            
            if (e.ColIndex > 0 && IsContentRow(e.RowIndex,dataItems.Count))
            {
                e.Style.CellType = "GridInCell";
                T dataItem = dataItems[e.RowIndex - 1];
                e.Style.Control = (GridControl)_propertyReflector.GetValue(dataItem, BindingProperty);
                e.Handled = true;
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0 && e.RowIndex <= dataItems.Count)
            {
                T dataItem = dataItems[e.RowIndex - 1];
                _propertyReflector.SetValue(dataItem, BindingProperty, e.Style.Control);
                e.Handled = true;
            }
        }
    }
}