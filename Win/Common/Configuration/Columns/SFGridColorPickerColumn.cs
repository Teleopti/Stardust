using System.Collections.ObjectModel;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridColorPickerColumn<T> : SFGridColumnBase<T>
    {
        public SFGridColorPickerColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        {
        }

        public override int PreferredWidth
        {
            get { return 100; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "ColorPickerCell";
            e.Style.CellValueType = typeof(Color);
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }
    }
}