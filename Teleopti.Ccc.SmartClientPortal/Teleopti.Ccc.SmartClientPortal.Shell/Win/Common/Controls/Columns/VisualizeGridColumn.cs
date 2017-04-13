using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Columns
{
    public class VisualizeGridColumn<T> : ColumnBase<T>
    {
        private readonly string _headerText;

        public VisualizeGridColumn(string bindingProperty, string headerText) : base(bindingProperty,15)
        {
            _headerText = headerText;
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            SetUpSingleHeader(e);
        }

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
                e.Style.CellType = "Static";
                e.Style.Borders.All = GridBorder.Empty;
                e.Handled = true;
            }
        }

        public override void SaveCellInfo(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            e.Handled = true;
        }
    }
}
