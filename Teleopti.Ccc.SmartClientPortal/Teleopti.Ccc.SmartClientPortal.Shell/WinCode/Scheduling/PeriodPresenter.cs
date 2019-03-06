using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class PeriodPresenter : SchedulePresenterBase
    {
        public PeriodPresenter(IScheduleViewBase view, ISchedulerStateHolder schedulerState, IGridlockManager lockManager, ClipHandler<IScheduleDay> clipHandler, 
            SchedulePartFilter schedulePartFilter, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, IScheduleDayChangeCallback scheduleDayChangeCallback,
            IScheduleTag defaultScheduleTag, IUndoRedoContainer undoRedoContainer, ITimeZoneGuard timeZoneGuard)
            : base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder,scheduleDayChangeCallback,
            defaultScheduleTag, undoRedoContainer, timeZoneGuard)
        {
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public override void QueryCellInfo(object sender, Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs e)
		{
			base.QueryCellInfo(sender, e);
			if (e.ColIndex > (int)ColumnType.StartScheduleColumns -1 && e.RowIndex > 1)
				e.Style.CellType = "Static";
		}
    }
}