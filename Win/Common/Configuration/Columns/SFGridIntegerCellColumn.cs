using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridIntegerCellColumn<T> : SFGridColumnBase<T>
    {
        private readonly IComparer<T> _columnComparer;

        public SFGridIntegerCellColumn(string bindingProperty, string headerText, IComparer<T> columnComparer)
            : base(bindingProperty, headerText)
        {
            _columnComparer = columnComparer;
        }

        public override IComparer<T> ColumnComparer
        {
            get
            {
                return _columnComparer;
            }
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
