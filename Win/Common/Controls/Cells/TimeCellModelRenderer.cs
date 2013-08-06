using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    public class TimeCellModelRenderer : GridTextBoxCellRenderer
    {
        public TimeCellModelRenderer(GridControlBase grid, GridTextBoxCellModel cellModel)
            : base(grid, cellModel)
        {
        }

        public override void OnPrepareViewStyleInfo(GridPrepareViewStyleInfoEventArgs e)
        {
            e.Style.HorizontalAlignment = GridHorizontalAlignment.Right;
            e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            e.Style.WrapText = false;
        }
    }
}