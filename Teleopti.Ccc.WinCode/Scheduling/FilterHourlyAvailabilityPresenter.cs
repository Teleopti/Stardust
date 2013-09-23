using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
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
            var persons = _schedulerStateHolder.FilteredPersonDictionary.Values;
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
