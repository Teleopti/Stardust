using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.IslandScheduling
{
	public class IslandScheduleConsolidater
	{
		public void Consolidate(IScheduleDictionary schedulingScreenDictionary, IScheduleDictionary islandDictionary)
		{
			foreach (var rangeToCopyFrom in islandDictionary.Values)
			{
				var allSchedules =
					rangeToCopyFrom.ScheduledDayCollection(
						rangeToCopyFrom.Period.ToDateOnlyPeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone));

				foreach (var scheduleDay in allSchedules)
				{
					var range = (ScheduleRange)schedulingScreenDictionary[rangeToCopyFrom.Person];
					range.ModifyInternal(scheduleDay);
					//schedulingScreenDictionary.Modify(ScheduleModifier.Scheduler, scheduleDay, NewBusinessRuleCollection.Minimum(),
					//new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());
				}
			}
		}
	}
}