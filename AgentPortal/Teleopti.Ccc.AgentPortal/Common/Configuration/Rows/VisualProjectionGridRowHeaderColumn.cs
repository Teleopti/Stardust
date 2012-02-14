using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common.Controls.Columns;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Rows
{
    public class VisualProjectionGridRowHeaderColumn : ScheduleGridColumnBase<VisualProjection>
    {
        public VisualProjectionGridRowHeaderColumn() : base("", "") { }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, IList<VisualProjection> dataItems, VisualProjection currentItem)
        { 
            e.Style.Tag = currentItem;
            e.Style.CellValue = currentItem.AgentName;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, IList<VisualProjection> dataItems, VisualProjection currentItem)
        {
        }
    }
}