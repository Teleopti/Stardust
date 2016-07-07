using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridIntegerCellColumn<T> : SFGridColumnBase<T>
    {
        public SFGridIntegerCellColumn(string bindingProperty, string headerText)
            : base(bindingProperty, headerText)
        {
        }

        public override int PreferredWidth
        {
            get { return 50; }
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "IntegerCellModel";
            e.Style.CellValueType = typeof(Int32);
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }
    }
}
