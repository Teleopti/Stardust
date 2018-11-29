using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Restriction;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class FilterOvertimeAvailabilityPresenter
	{
		private readonly ISchedulerStateHolder _schedulerStateHolder;
	    private readonly List<IScheduleDay> _schedules = new List<IScheduleDay>(); 

		public FilterOvertimeAvailabilityPresenter(ISchedulerStateHolder schedulerStateHolder)
		{
		    _schedulerStateHolder = schedulerStateHolder;
		}

		public void Filter(TimePeriod filterPeriod, DateOnly value, bool allowIntersect)
		{
            var persons = _schedulerStateHolder.FilteredCombinedAgentsDictionary.Values;
            foreach (var person in persons)
            {
                var scheduleDays = _schedulerStateHolder.Schedules[person].ScheduledDayCollection(new DateOnlyPeriod(value, value));
                _schedules.AddRange(scheduleDays);
            }
            var filter = new OvertimeAvailabilityPersonFilter(new TimeZoneGuard());
			var filteredPersons = filter.GetFilteredPerson(_schedules, filterPeriod, allowIntersect);
			_schedulerStateHolder.FilterPersonsOvertimeAvailability(filteredPersons);
		}
	}
}