using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common.Controls.Columns;
using Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Rows
{
    public class ShiftTradeVisualRowHeaderColumn : ScheduleGridColumnBase<ShiftTradeDetailModel>
    {
        public ShiftTradeVisualRowHeaderColumn() : base("", "") { }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, IList<ShiftTradeDetailModel> dataItems, ShiftTradeDetailModel currentItem)
        {
            if (currentItem == null) return;
            if (e.ColIndex == 0)
            {
                e.Style.CellValue = currentItem.TradeDate.ToShortDateString();
                e.Style.MergeCell = GridMergeCellDirection.RowsInColumn;
                return;
            }
            e.Style.Tag = currentItem.VisualProjection;
            e.Style.CellValue = currentItem.Person;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, IList<ShiftTradeDetailModel> dataItems, ShiftTradeDetailModel currentItem)
        {
        }
    }
}