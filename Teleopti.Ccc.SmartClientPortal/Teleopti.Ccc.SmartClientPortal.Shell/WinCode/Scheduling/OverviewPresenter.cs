using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class OverviewPresenter : SchedulePresenterBase
    {
        public OverviewPresenter(IScheduleViewBase view, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
            ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, 
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag, IUndoRedoContainer undoRedoContainer, ITimeZoneGuard timeZoneGuard)
            : base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder,
            scheduleDayChangeCallback, defaultScheduleTag, undoRedoContainer, timeZoneGuard)
        {
        }
    }
}