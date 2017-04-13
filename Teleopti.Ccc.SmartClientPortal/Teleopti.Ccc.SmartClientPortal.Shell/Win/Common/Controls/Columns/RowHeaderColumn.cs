using Syncfusion.Windows.Forms.Grid;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Win.Common.Controls.Columns
{
    public class RowHeaderColumn<T> : ColumnBase<T>
    {
        public RowHeaderColumn() : base(string.Empty,25)
        {
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Handled = true;
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Handled = true;
        }
    }
}