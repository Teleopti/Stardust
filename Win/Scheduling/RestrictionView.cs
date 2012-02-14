using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class RestrictionView : ScheduleViewBase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public RestrictionView(GridControl grid, SchedulerStateHolder schedulerState, GridlockManager lockManager,
            SchedulePartFilter schedulePartFilter, ClipHandler<IScheduleDay> clipHandler, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, 
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
            : base(grid)
        {
            Presenter = new RestrictionPresenter(this, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, 
                scheduleDayChangeCallback, defaultScheduleTag);
            Presenter.VisibleWeeks = 1;
            if (!grid.Model.CellModels.ContainsKey("RestrictionCell"))
                grid.Model.CellModels.Add("RestrictionCell", new RestrictionViewCellModel(grid.Model));
            grid.HideCols.SetRange(2, 5, true);
        }

        protected override int CellWidth()
        {
            return 105;
        }

        internal override void QueryRowHeight(object sender, GridRowColSizeEventArgs e)
        {
            if (e.Index > 1)
            {
                e.Size = 60;
                e.Handled = true;
            }
        }

        internal override void CellDrawn(object sender, GridDrawCellEventArgs e)
        {
            base.CellDrawn(sender, e);

            if (e.RowIndex > 1 && e.ColIndex > ColHeaders)
            {
                RestrictionCellValue cellValue = e.Style.CellValue as RestrictionCellValue;
                if (cellValue != null)
                {
                    if (cellValue.SchedulePart != null)
                    {
                        AddMarkersToCell(e, cellValue.SchedulePart, cellValue.SchedulePart.SignificantPart());
                    }
                }
            }
            
        }
    }
}