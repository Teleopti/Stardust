using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class ScheduleDictionaryForTest : ScheduleDictionary
	{
		public ScheduleDictionaryForTest(IScenario scenario, DateTime date)
			: this(scenario, new DateTimePeriod(cloneToUtc(date.Date), cloneToUtc(date.Date.AddHours(24)))) { }

		public ScheduleDictionaryForTest(IScenario scenario, DateTime startDate, DateTime endDate)
			: this(scenario, new DateTimePeriod(cloneToUtc(startDate.Date), cloneToUtc(endDate.Date.AddHours(24)))) { }

		public ScheduleDictionaryForTest(IScenario scenario, IScheduleDateTimePeriod period, IDictionary<IPerson, IScheduleRange> dictionary)
			: base(scenario, period, dictionary) { }

		public ScheduleDictionaryForTest(IScenario scenario, DateTimePeriod period)
			: base(scenario, new ScheduleDateTimePeriod(period), new Dictionary<IPerson, IScheduleRange>()) { }

		public static IScheduleDictionary WithPersonAssignment(IScenario scenario, DateTimePeriod period, IPersonAssignment personAssignment)
		{
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, period);
			var scheduleRange = new ScheduleRange(scheduleDictionary, new ScheduleParameters(scenario, personAssignment.Person, period));
			scheduleRange.Add(personAssignment);
			scheduleDictionary.BaseDictionary.Add(personAssignment.Person, scheduleRange);
			scheduleDictionary.TakeSnapshot();
			return scheduleDictionary;
		}

		public void AddTestItem(IPerson person, IScheduleRange range)
		{
			BaseDictionary.Add(person, range);
		}



		private static DateTime cloneToUtc(DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Utc);
		}

	}
}
