using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Common;
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

		public void Initialize()
		{
			var persons = _schedulerStateHolder.FilteredPersonDictionary.Values;
			foreach (var person in persons)
			{
				var loadedPeriod = _schedulerStateHolder.RequestedPeriod.DateOnlyPeriod;
				var scheduleDays = _schedulerStateHolder.Schedules[person].ScheduledDayCollection(loadedPeriod);
				_schedules.AddRange(scheduleDays);
			}
		}

		public void Filter(TimeSpan filterStartTime, TimeSpan filterEndTime)
		{
			var filter = new OvertimeAvailabilityPersonFilter();
			var filteredPersons = filter.GetFilterdPerson(_schedules, filterStartTime, filterEndTime);
			_schedulerStateHolder.FilterPersonsOvertimeAvailability(filteredPersons);
		}
	}
}