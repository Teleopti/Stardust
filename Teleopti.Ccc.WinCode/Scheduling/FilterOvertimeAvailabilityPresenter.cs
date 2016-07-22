﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class FilterOvertimeAvailabilityPresenter
	{
		private readonly ISchedulerStateHolder _schedulerStateHolder;
	    private readonly List<IScheduleDay> _schedules = new List<IScheduleDay>(); 

		public FilterOvertimeAvailabilityPresenter(ISchedulerStateHolder schedulerStateHolder)
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
            var filter = new OvertimeAvailabilityPersonFilter();
			var filteredPersons = filter.GetFilterdPerson(_schedules, filterStartTime, filterEndTime, _schedulerStateHolder.TimeZoneInfo, value);
			_schedulerStateHolder.FilterPersonsOvertimeAvailability(filteredPersons);
		}
	}
}