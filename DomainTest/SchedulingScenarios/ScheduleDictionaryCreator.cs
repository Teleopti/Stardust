using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public static class ScheduleDictionaryCreator
	{
		//TODO: remove
		public static IScheduleDictionary WithData(IScenario scenario,
			DateOnlyPeriod period,
			IEnumerable<IScheduleData> persistableScheduleData)
		{
			var dateTimePeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
			var ret = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(dateTimePeriod), new PersistableScheduleDataPermissionChecker());
			foreach (var scheduleData in persistableScheduleData)
			{
				((ScheduleRange)ret[scheduleData.Person]).Add(scheduleData);
			}
			return ret;
		}

		public static IScheduleDictionary WithData(IScenario scenario,
			IEnumerable<IPerson> agents,
			DateOnlyPeriod period,
			IEnumerable<IScheduleData> persistableScheduleData)
		{
			var dateTimePeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
			var ret = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(dateTimePeriod, agents), new PersistableScheduleDataPermissionChecker());
			foreach (var scheduleData in persistableScheduleData)
			{
				((ScheduleRange)ret[scheduleData.Person]).Add(scheduleData);
			}
			return ret;
		}
	}
}