using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace GridTest
{
    public class VisualProjectionGridRowHeaderColumn : ScheduleGridColumnBase<VisualProjection>
    {
        public VisualProjectionGridRowHeaderColumn() :base("", ""){}

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<VisualProjection> dataItems, VisualProjection currentItem)
        {
            e.Style.CellValue = currentItem.AgentName;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<VisualProjection> dataItems, VisualProjection currentItem)
        {
        }
    }
}