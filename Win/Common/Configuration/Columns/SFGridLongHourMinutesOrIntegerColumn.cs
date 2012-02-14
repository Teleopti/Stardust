using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridHourMinutesOrIntegerColumn<T> : SFGridColumnBase<T>
    {
        public SFGridHourMinutesOrIntegerColumn(string bindingProperty, string headerText) : base(bindingProperty, headerText)
        {
        }

        public override int PreferredWidth
        {
            get { return 150; }
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            base.GetCellInfo(e, dataItems);
        	if (e.Handled) return;
        	GetCellValue(e, dataItems, dataItems[e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1)]);
        	e.Style.CellType = e.Style.CellValueType == typeof(double) ? "NumericCell" : "TimeSpanLongHourMinutesCell";
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }
    }
}
