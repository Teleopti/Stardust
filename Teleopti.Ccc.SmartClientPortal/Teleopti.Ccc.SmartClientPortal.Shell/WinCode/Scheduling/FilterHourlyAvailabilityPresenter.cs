using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Restriction;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class FilterHourlyAvailabilityPresenter
	{
		private readonly ISchedulerStateHolder _schedulerStateHolder;
	    private readonly List<IScheduleDay> _schedules = new List<IScheduleDay>();

		public FilterHourlyAvailabilityPresenter(ISchedulerStateHolder schedulerStateHolder)
		{
		    _schedulerStateHolder = schedulerStateHolder;
		}

		public void Filter(TimeSpan filterStartTime, TimeSpan filterEndTime, DateOnly value)
		{
            var persons = _schedulerStateHolder.FilteredCombinedAgentsDictionary.Values;
            foreach (var person in persons)
            {
                var scheduleDays = _schedulerStateHolder.Schedules[person].ScheduledDayCollection(new DateOnlyPeriod(value, value));
                _schedules.AddRange(scheduleDays);
            }
            var filter = new HourlyAvailabilityPersonFilter();
			var filteredPersons = filter.GetFilterdPerson(_schedules, filterStartTime, filterEndTime);
			_schedulerStateHolder.FilterPersonsHourlyAvailability(filteredPersons);
		}
	}
}
