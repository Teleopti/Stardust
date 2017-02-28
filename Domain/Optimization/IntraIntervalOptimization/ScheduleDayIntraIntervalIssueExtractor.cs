﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface IScheduleDayIntraIntervalIssueExtractor
	{
		IList<IScheduleDay> Extract(IScheduleDictionary scheduleDictionary, DateOnly dateOnly, IList<ISkillStaffPeriod> issues);
	}

	public class ScheduleDayIntraIntervalIssueExtractor : IScheduleDayIntraIntervalIssueExtractor
	{
		public IList<IScheduleDay> Extract(IScheduleDictionary scheduleDictionary, DateOnly dateOnly, IList<ISkillStaffPeriod> issues)
		{
			var result = new List<IScheduleDay>();

			foreach (var schedule in scheduleDictionary)
			{
				var scheduleDay = schedule.Value.ScheduledDay(dateOnly);
				checkDay(scheduleDay, result, issues);
			}

			return result;
		}

		private static void checkDay(IScheduleDay scheduleDay, IList<IScheduleDay> result, IEnumerable<ISkillStaffPeriod> issues)
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
