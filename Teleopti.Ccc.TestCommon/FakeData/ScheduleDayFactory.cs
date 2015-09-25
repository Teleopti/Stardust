using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

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
			var ret = ExtractedSchedule.CreateScheduleDay(scheduleDictionary, parameters.Person, dateOnly);
			return ret;
		}


		 public static IScheduleDay Create(DateOnly dateOnly)
		 {
			 var person = new Person();
			 return Create (dateOnly, person);

		 }
	}
}