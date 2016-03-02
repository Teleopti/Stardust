using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class OverviewPresenter : SchedulePresenterBase
    {
        public OverviewPresenter(IScheduleViewBase view, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
            ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, 
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
            : base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder,
            scheduleDayChangeCallback, defaultScheduleTag)
        {
        }
    }
}