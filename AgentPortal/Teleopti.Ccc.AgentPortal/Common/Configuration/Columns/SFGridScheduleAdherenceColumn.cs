using System;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public class SFGridScheduleAdherenceColumn<T> : SFGridColumnBase<T>
    {
        private TimePeriod _timePeriod = new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(18));

        public SFGridScheduleAdherenceColumn(string bindingProperty, string headerText, int preferredWidth)
            : base(bindingProperty, headerText, preferredWidth)
        { }

        public TimePeriod TimePeriod
        {
            get { return _timePeriod; }
            set { _timePeriod = value; }
        }

        public override void GetCellInfo(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems)
        {
            if (e.Style.CellModel == null)
                return;

            e.Style.Tag = _timePeriod;
            if (e.RowIndex <= e.Style.CellModel.Grid.Rows.HeaderCount)
            {
                e.Style.CellType = "VisualProjectionColumnHeaderCell";
                e.Style.CellValue = string.Empty;
            }
            else
            {
                GetCellValue(e, dataItems, dataItems[e.RowIndex - (e.Style.CellModel.Grid.Rows.HeaderCount + 1)]);
            }

            e.Handled = true;
        }

        public override void GetCellValue(GridQueryCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            e.Style.CellType = "ScheduleAdherenceCell";
            e.Style.CellValue = PropertyReflectorHelper.GetValue(currentItem, BindingProperty);
            e.Handled = true;
        }

        public override void SaveCellValue(GridSaveCellInfoEventArgs e, ReadOnlyCollection<T> dataItems, T currentItem)
        {
            throw new NotImplementedException();
        }

        public override int PreferredWidth
        {
            get
            {
                double minWidth = _timePeriod.SpanningTime().TotalMinutes/15*20;
                return (int) minWidth;
                //return base.PreferredWidth;
            }
            set
            {
                base.PreferredWidth = value;
            }
        }
    }
}
