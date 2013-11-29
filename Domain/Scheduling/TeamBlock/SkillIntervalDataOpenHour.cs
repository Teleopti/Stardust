using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISkillIntervalDataOpenHour
	{
		TimePeriod GetOpenHours(IList<ISkillIntervalData> skillIntervalDataList, DateOnly skillDayDate);
	}

	public class SkillIntervalDataOpenHour : ISkillIntervalDataOpenHour
	{

		public TimePeriod GetOpenHours(IList<ISkillIntervalData> skillIntervalDataList, DateOnly skillDayDate)
		{

			var minDateTime = (from o in skillIntervalDataList
							   select o.Period.StartDateTime).Min();
			var maxDateTime = (from o in skillIntervalDataList
							   select o.Period.EndDateTime).Max();

			var minTime = minDateTime.TimeOfDay;
			int minDayOffset = (int)minDateTime.Date.Subtract(skillDayDate).TotalDays;
			minTime = minTime.Add(TimeSpan.FromDays(minDayOffset));

			var maxTime = maxDateTime.TimeOfDay;
			int maxDayOffset = (int)maxDateTime.Date.Subtract(skillDayDate).TotalDays;
			maxTime = maxTime.Add(TimeSpan.FromDays(maxDayOffset));

			return new TimePeriod(minTime, maxTime);
		}
	}
}