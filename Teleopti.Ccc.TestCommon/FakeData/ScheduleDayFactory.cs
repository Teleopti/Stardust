using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class ScheduleDayFactory
	{

		public static IScheduleDay Create (DateOnly dateOnly, IPerson person, IScenario scenario = null)
		{
			scenario = scenario ?? new Scenario("d");
			var period = new DateTimePeriod(1800, 1, 1, 2100, 1, 1);
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(period), new Dictionary<IPerson, IScheduleRange>());
			var parameters = new ScheduleParameters(scenario, person, period);
			var ret = ExtractedSchedule.CreateScheduleDay(scheduleDictionary, parameters.Person, dateOnly, new FullPermission());
			return ret;
		}


		 public static IScheduleDay Create(DateOnly dateOnly)
		 {
			 var person = new Person();
			 return Create (dateOnly, person);

		 }

		public static IScheduleDay Create(DateOnly dateOnly, TimeZoneInfo timeZoneInfo)
		{
			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
			return Create(dateOnly, person);
		}
	}
}