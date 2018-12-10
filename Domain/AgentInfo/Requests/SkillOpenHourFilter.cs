using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class SkillOpenHourFilter : ISkillOpenHourFilter
	{
		public IEnumerable<ISkill> Filter(DateTimePeriod requestPeriod, IEnumerable<ISkill> skills)
		{
			var openSkills = new List<ISkill>();
			foreach (var skill in skills)
			{
				var dayTimePeriods = getDayTimePeriods(requestPeriod, skill.TimeZone);
				var isSkillOpenInDays = dayTimePeriods.All(day => isSkillOpen(skill, (int)day.Key.DayOfWeek, day.Value));
				if (isSkillOpenInDays)
				{
					openSkills.Add(skill);
				}
			}
			return openSkills;
		}

		private bool isSkillOpen(ISkill skill, int weekDay, TimePeriod requestTimePeriod)
		{
			foreach (var workload in skill.WorkloadCollection)
			{
				if (workload.TemplateWeekCollection.ContainsKey(weekDay)
					&& workload.TemplateWeekCollection[weekDay].OpenForWork.IsOpen)
				{
					return workload.TemplateWeekCollection[weekDay]
						.OpenHourList.Any(openHour => openHour.Contains(requestTimePeriod));
				}
			}
			return false;
		}

		private Dictionary<DateOnly, TimePeriod> getDayTimePeriods(DateTimePeriod requestPeriod, TimeZoneInfo timezone)
		{
			var dayTimePeriods = new Dictionary<DateOnly, TimePeriod>();
			var startTime = requestPeriod.StartDateTimeLocal(timezone);
			var endTime = requestPeriod.EndDateTimeLocal(timezone);
			if (startTime.Date == endTime.Date)
			{
				dayTimePeriods.Add(new DateOnly(startTime.Date), requestPeriod.TimePeriod(timezone));
			}
			else
			{
				dayTimePeriods.Add(new DateOnly(startTime.Date),
					new TimePeriod(startTime.TimeOfDay, TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(1))));
				dayTimePeriods.Add(new DateOnly(endTime.Date), new TimePeriod(TimeSpan.Zero, endTime.TimeOfDay));
			}
			return dayTimePeriods;
		}
	}
}