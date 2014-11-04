using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IScheduleDayIntraIntervalIssueExtractor
	{
		IList<IScheduleDay> Extract(IScheduleDictionary scheduleDictionary, DateOnly dateOnly, IList<ISkillStaffPeriod> issues, ISkill skill);
	}

	public class ScheduleDayIntraIntervalIssueExtractor : IScheduleDayIntraIntervalIssueExtractor
	{
		public IList<IScheduleDay> Extract(IScheduleDictionary scheduleDictionary, DateOnly dateOnly, IList<ISkillStaffPeriod> issues, ISkill skill)
		{
			var result = new List<IScheduleDay>();

			foreach (var schedule in scheduleDictionary)
			{
				var scheduleDay = schedule.Value.ScheduledDay(dateOnly);
				checkDay(scheduleDay, result, issues, skill);
			}

			return result;
		}

		private void checkDay(IScheduleDay scheduleDay, IList<IScheduleDay> result, IList<ISkillStaffPeriod> issues, ISkill skill)
		{
			if (!scheduleDay.SignificantPart().Equals(SchedulePartView.MainShift)) return;

			var projection = scheduleDay.ProjectionService().CreateProjection();

			foreach (var skillStaffPeriod in issues)
			{
				var found = false;

				foreach (var visualLayer in projection)
				{
					if (visualLayer.Period.Contains(skillStaffPeriod.Period)) continue;
					if (visualLayer.Period.Intersect(skillStaffPeriod.Period))
					{
						var activity = visualLayer.Payload as Activity;
						if (activity != null && activity.Equals(skill.Activity)) continue;
						result.Add(scheduleDay);
						found = true;
						break;
					}
				}

				if (found) break;
			}
		}
	}
}
