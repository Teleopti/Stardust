﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class ScheduleDictionaryForTest : ScheduleDictionary, IReadOnlyScheduleDictionary
	{
		public ScheduleDictionaryForTest(IScenario scenario, DateTime date)
			: this(scenario, new DateTimePeriod(DateTime.SpecifyKind(date.Date,DateTimeKind.Utc), DateTime.SpecifyKind(date.Date.AddHours(24),DateTimeKind.Utc))) { }

		public ScheduleDictionaryForTest(IScenario scenario, DateTime startDate, DateTime endDate)
			: this(scenario, new DateTimePeriod(DateTime.SpecifyKind(startDate.Date,DateTimeKind.Utc), DateTime.SpecifyKind(endDate.Date.AddHours(24),DateTimeKind.Utc))) { }

		public ScheduleDictionaryForTest(IScenario scenario, IScheduleDateTimePeriod period, IDictionary<IPerson, IScheduleRange> dictionary)
			: base(scenario, period, dictionary, new PersistableScheduleDataPermissionChecker()) { }

		public ScheduleDictionaryForTest(IScenario scenario, DateTimePeriod period)
			: base(scenario, new ScheduleDateTimePeriod(period), new Dictionary<IPerson, IScheduleRange>(), new PersistableScheduleDataPermissionChecker()) { }



		public static IScheduleDictionary WithPersonAssignment(IScenario scenario, DateTime date, IPersonAssignment personAssignment)
		{
			return WithPersonAssignment(scenario, new DateTimePeriod(DateTime.SpecifyKind(date.Date,DateTimeKind.Utc), DateTime.SpecifyKind(date.Date.AddHours(24),DateTimeKind.Utc)), personAssignment);
		}

		public static IScheduleDictionary WithPersonAssignment(IScenario scenario, DateTimePeriod period, IPersonAssignment personAssignment)
		{
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, period);
			scheduleDictionary.AddPersonAssignment(personAssignment);
			return scheduleDictionary;
		}

		public void AddPersonAssignment(IPersonAssignment personAssignment)
		{
			var person = personAssignment.Person;
			if (BaseDictionary.ContainsKey(person))
			{
				((ScheduleRange)BaseDictionary[person]).Add(personAssignment);
			}
			else
			{
				var scheduleRange = new ScheduleRange(this, new ScheduleParameters(Scenario, person, Period.VisiblePeriod), new PersistableScheduleDataPermissionChecker());
				scheduleRange.Add(personAssignment);
				BaseDictionary[person] = scheduleRange;
			}
			TakeSnapshot();
		}

		public static IScheduleDictionary WithPersonAbsence(IScenario scenario, DateTimePeriod period, IPersonAbsence personAbsence)
		{
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, period);
			scheduleDictionary.AddPersonAbsence(personAbsence);
			return scheduleDictionary;
		}

		public void AddPersonAbsence(IPersonAbsence personAbsence)
		{
			var person = personAbsence.Person;
			if (BaseDictionary.ContainsKey(person))
			{
				((ScheduleRange)BaseDictionary[person]).Add(personAbsence);		
			}
			else
			{
				var scheduleRange = new ScheduleRange(this, new ScheduleParameters(Scenario, personAbsence.Person, Period.VisiblePeriod), new PersistableScheduleDataPermissionChecker());
				scheduleRange.Add(personAbsence);
				BaseDictionary.Add(personAbsence.Person, scheduleRange);
				TakeSnapshot();	
			}	
		}

		public static IScheduleDictionary WithScheduleData(IPerson person, IScenario scenario, DateTimePeriod period, params IScheduleData[] data)
		{
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, period);
			if (data != null)
			{
				scheduleDictionary.AddScheduleData(person, data);
			}
			return scheduleDictionary;
		}

		public void AddScheduleData(params IScheduleData[] data)
		{
			AddScheduleData(data.First().Person, data);
		}
					 
		public void AddScheduleData(IPerson person, params IScheduleData[] data)
		{
			var scheduleRange = new ScheduleRange(this, new ScheduleParameters(Scenario, person, Period.VisiblePeriod), new PersistableScheduleDataPermissionChecker());
			scheduleRange.AddRange(data);
			BaseDictionary.Add(person, scheduleRange);
			TakeSnapshot();
		}

		public static IScheduleDictionary WithScheduleDataForManyPeople(IScenario scenario, DateTimePeriod period, params IScheduleData[] data)
		{
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, period);
			scheduleDictionary.AddScheduleDataManyPeople(data);
			return scheduleDictionary;
		}

		public void AddScheduleDataManyPeople(params IScheduleData[] data)
		{
			foreach (var scheduleData in data)
			{
				IScheduleRange scheduleRange;

				if (!BaseDictionary.TryGetValue (scheduleData.Person, out scheduleRange))
				{
					scheduleRange = new ScheduleRange(this, new ScheduleParameters(Scenario, scheduleData.Person, Period.VisiblePeriod), new PersistableScheduleDataPermissionChecker());
					BaseDictionary.Add(scheduleData.Person, scheduleRange);
				}

				var timezone = scheduleData.Person.PermissionInformation.DefaultTimeZone();
				if (scheduleData.BelongsToPeriod(Period.VisiblePeriod.ToDateOnlyPeriod(timezone)))
				{
					((ScheduleRange)scheduleRange).Add(scheduleData);
				}
			}
			
			TakeSnapshot();
		}

		public void AddTestItem(IPerson person, IScheduleRange range)
		{
			BaseDictionary.Add(person, range);
		}
		
		public void MakeEditable()
		{
			//throw new NotImplementedException();
		}
	}
}
