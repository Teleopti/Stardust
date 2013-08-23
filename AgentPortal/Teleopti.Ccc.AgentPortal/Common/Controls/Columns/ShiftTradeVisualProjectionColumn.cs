using System.Collections.Generic;
using System.Drawing;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Common.Controls.Columns
{
    public class ShiftTradeVisualProjectionColumn : ScheduleGridColumnBase<ShiftTradeDetailModel>
    {
        private readonly IComparer<ShiftTradeDetailModel> _columnComparer;

        public ShiftTradeVisualProjectionColumn(string bindingProperty, string headerText, IComparer<ShiftTradeDetailModel> columnComparer)
            : base(bindingProperty, headerText)
        {
            _columnComparer = columnComparer;
        }

        public override IComparer<ShiftTradeDetailModel> ColumnComparer
        {
            get
            {
                return _columnComparer;
            }
        }

        public string DisplayDate { get; set; }

        public TimePeriod Period { get; set; }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, IList<ShiftTradeDetailModel> dataItems)
        {
            e.Style.Tag = Period;
            if (e.RowIndex <= e.Style.CellModel.Grid.Rows.HeaderCount)
            {
                e.Style.CellType = "VisualProjectionColumnHeaderCell";
                e.Style.CellValue = DisplayDate;
                //e.Style.CellValue = string.Empty;
            }
            else
            {
                GetCellValue(e, dataItems, dataItems[e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1)]);
            }

            e.Handled = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString")]
        public override void GetCellValue(GridQueryCellInfoEventArgs e, IList<ShiftTradeDetailModel> dataItems, ShiftTradeDetailModel currentItem)
        {
            if (currentItem == null) return;

            VisualProjection visualProjection = currentItem.VisualProjection;
            if (!visualProjection.IsDayOff)
            {
                e.Style.CellType = "VisualProjectionCell";
                e.Style.CellValue = visualProjection.LayerCollection;
            	e.Style.CellValueType = typeof (ActivityVisualLayer);
            }
            else
            {
                e.Style.CellType = "StaticCellModel";
                e.Style.Tag = Period;
                e.Style.CellValue = visualProjection.DayOffName;
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            }

            int rowPair = (e.RowIndex-(e.Style.CellModel.Grid.Rows.HeaderCount + 1)) % 4;
            if (rowPair>1)
            {
                e.Style.BackColor = Color.FromArgb(240,240,240); //Really light gray
            }
            if (rowPair == 1 || rowPair == 3)
            {
                //Add an extra separator between the pairs of shift
                e.Style.Borders.Bottom = new GridBorder(GridBorderStyle.Solid, e.Style.Borders.Top.Color,
                                                     GridBorderWeight.Medium);
            }
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, IList<ShiftTradeDetailModel> dataItems, ShiftTradeDetailModel currentItem)
        {
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }
    }
}