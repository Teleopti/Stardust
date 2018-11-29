using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public class ScheduleDayIntraIntervalIssueExtractor
	{
		public IEnumerable<IScheduleDay> Extract(IScheduleDictionary scheduleDictionary, DateOnly dateOnly, IEnumerable<ISkillStaffPeriod> issues)
		{
			var result = new List<IScheduleDay>();

			foreach (var schedule in scheduleDictionary)
			{
				var scheduleDay = schedule.Value.ScheduledDay(dateOnly);
				checkDay(scheduleDay, result, issues);
			}

			return result;
		}

		private static void checkDay(IScheduleDay scheduleDay, ICollection<IScheduleDay> result, IEnumerable<ISkillStaffPeriod> issues)
		{
			if (!scheduleDay.SignificantPart().Equals(SchedulePartView.MainShift)) return;

			foreach (var skillStaffPeriod in issues)
			{
				if (scheduleDay.PersonAssignment().Period.Intersect(skillStaffPeriod.Period))
				{
					result.Add(scheduleDay);
					break;
				}		
			}
		}
	}
}
