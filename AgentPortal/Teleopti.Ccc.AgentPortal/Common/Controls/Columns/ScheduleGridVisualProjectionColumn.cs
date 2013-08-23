using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Common.Controls.Columns
{
    public class ScheduleGridVisualProjectionColumn : ScheduleGridColumnBase<VisualProjection>
    {
        private readonly IComparer<VisualProjection> _columnComparer;

        public ScheduleGridVisualProjectionColumn(string bindingProperty, string headerText, IComparer<VisualProjection> columnComparer)
            : base(bindingProperty, headerText)
        {
            _columnComparer = columnComparer;
        }

        public override IComparer<VisualProjection> ColumnComparer
        {
            get
            {
                return _columnComparer;
            }
        }

        public string DisplayDate { get; set; }

        public TimePeriod Period { get; set; }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, IList<VisualProjection> dataItems)
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
                int index = e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1);
                if (index<dataItems.Count)
                    GetCellValue(e, dataItems, dataItems[index]);
            }

            e.Handled = true;
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, IList<VisualProjection> dataItems, VisualProjection currentItem)
        {
            if (currentItem == null)
                return;
            if (!currentItem.IsDayOff)
            {
                e.Style.CellType = "VisualProjectionCell";
                e.Style.CellValue = currentItem.LayerCollection;
                e.Style.CellValueType = typeof(ActivityVisualLayer);
            }
            else
            {
                e.Style.CellType = "StaticCellModel";
                e.Style.Tag = Period;
                e.Style.CellValue = currentItem.DayOffName;
                e.Style.HorizontalAlignment = GridHorizontalAlignment.Center;
                e.Style.VerticalAlignment = GridVerticalAlignment.Middle;
            }
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, IList<VisualProjection> dataItems, VisualProjection currentItem)
        {
            PropertyReflectorHelper.SetValue(currentItem, BindingProperty, e.Style.CellValue);
        }
    }
}