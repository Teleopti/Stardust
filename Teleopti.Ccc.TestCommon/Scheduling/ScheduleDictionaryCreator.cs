using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.TestCommon.Scheduling
{
	public static class ScheduleDictionaryCreator
	{
		public static IScheduleDictionary WithData(IScenario scenario,
			DateOnlyPeriod period, ICurrentAuthorization currentAuthorization = null)
		{
			return WithData(scenario, period, Enumerable.Empty<IScheduleData>(), currentAuthorization);
		}
		
		public static IScheduleDictionary WithData(IScenario scenario,
			DateOnlyPeriod period,
			IEnumerable<IScheduleData> persistableScheduleData, ICurrentAuthorization currentAuthorization = null)
		{
			return WithData(scenario, period, persistableScheduleData, persistableScheduleData.Select(x => x.Person), currentAuthorization);
		}

		public static IScheduleDictionary WithData(IScenario scenario,
			DateOnlyPeriod period,
			IEnumerable<IScheduleData> persistableScheduleData,
			IEnumerable<IPerson> agents,
			ICurrentAuthorization currentAuthorization = null)
		{
			var dateTimePeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
			var ret = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(dateTimePeriod, agents), new PersistableScheduleDataPermissionChecker(currentAuthorization ?? new FullPermission()), currentAuthorization ?? new FullPermission());
			using (TurnoffPermissionScope.For(ret))
			{
				foreach (var scheduleData in persistableScheduleData)
				{
					((ScheduleRange)ret[scheduleData.Person]).Add(scheduleData);
				}
				return ret;				
			}
		}
	}
}