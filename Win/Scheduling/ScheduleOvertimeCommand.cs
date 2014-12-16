using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public interface IScheduleOvertimeCommand
    {
        void Exectue(IOvertimePreferences overtimePreferences, BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedSchedules, IResourceCalculateDelayer resourceCalculateDelayer, IGridlockManager gridlockManager);
    }

    public class ScheduleOvertimeCommand : IScheduleOvertimeCommand
    {
        private readonly ISchedulerStateHolder _schedulerState;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IScheduleOvertimeService _scheduleOvertimeService;
	    private BackgroundWorker _backgroundWorker;
	    private SchedulingServiceBaseEventArgs _progressEvent;

        public ScheduleOvertimeCommand(ISchedulerStateHolder schedulerState, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleOvertimeService scheduleOvertimeService)
        {
            _schedulerState = schedulerState;
            _schedulingResultStateHolder = schedulingResultStateHolder;
	        _scheduleOvertimeService = scheduleOvertimeService;
        }

        public void Exectue(IOvertimePreferences overtimePreferences, BackgroundWorker backgroundWorker, IList<IScheduleDay> selectedSchedules, IResourceCalculateDelayer resourceCalculateDelayer, IGridlockManager gridlockManager)
        {
            _backgroundWorker = backgroundWorker;
	        _progressEvent = null;
            var selectedDates = selectedSchedules.Select(x => x.DateOnlyAsPeriod.DateOnly).Distinct();
            var selectedPersons = selectedSchedules.Select(x => x.Person).Distinct().ToList();
            foreach (var dateOnly in selectedDates)
            {
				var persons = selectedPersons.Randomize();
                foreach (var person in persons)
                {
                    if (checkIfCancelPressed()) return;
					if (_progressEvent != null && _progressEvent.UserCancel) return;

                    var locks = gridlockManager.Gridlocks(person, dateOnly);
                    if (locks != null && locks.Count != 0) continue;

                    var scheduleDay = _schedulingResultStateHolder.Schedules[person].ScheduledDay(dateOnly);
	                var rules = NewBusinessRuleCollection.Minimum();
					IScheduleTagSetter scheduleTagSetter = new ScheduleTagSetter(overtimePreferences.ScheduleTag);
	                _scheduleOvertimeService.SchedulePersonOnDay(scheduleDay, overtimePreferences, resourceCalculateDelayer, dateOnly, rules, scheduleTagSetter, _schedulerState.TimeZoneInfo);
					
                    OnDayScheduled(new SchedulingServiceSuccessfulEventArgs(scheduleDay));
                }
            }
        }

        protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs e)
        {
            if (_backgroundWorker.CancellationPending)
            {
                e.Cancel = true;
            }
			
			_backgroundWorker.ReportProgress(1, e);

			if (_progressEvent != null && _progressEvent.UserCancel)
				return;
			
			_progressEvent = e;
        }

		
        private bool checkIfCancelPressed()
        {
            if (_backgroundWorker.CancellationPending)
                return true;
            return false;
        }
    }
}
