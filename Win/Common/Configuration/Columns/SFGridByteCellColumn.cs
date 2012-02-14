using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridByteCellColumn<T> : SFGridColumnBase<T>
    {
        private readonly IComparer<T> _columnComparer;

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

        public SFGridByteCellColumn(string bindingProperty, string headerText, IComparer<T> columnComparer)
            : base(bindingProperty, headerText)
        {
            _columnComparer = columnComparer;
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "NumericCell";
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            byte value;

            if (byte.TryParse(e.Style.CellValue.ToString(), out value))
            {
                PropertyReflectorHelper.SetValue(currentItem, BindingProperty, value);
            }
        }

    }

}
