using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public class IsAnySkillOpen
	{
		public bool Check(IEnumerable<ISkillDay> skillDays, IVisualLayer layer, TimeZoneInfo agentTimeZoneInfo)
		{
			var layerActivity = (IActivity) layer.Payload;
			if (!layerActivity.RequiresSkill)
				return true;
			var layerStartDate = layer.Period.ToDateOnlyPeriod(agentTimeZoneInfo).StartDate;
			foreach (var skillDay in skillDays)
			{
				if (!(skillDay.Skill is MaxSeatSkill) &&
					skillDay.Skill.Activity.Equals(layerActivity) &&
					skillDay.CurrentDate.Equals(layerStartDate))
				{

					var skillTimeZone = skillDay.Skill.TimeZone;
					var skillDayOpenHours = skillDay.OpenHours().ToList();
					var layerTimePeriod = layer.Period.TimePeriod(skillTimeZone);
					if (skillDayOpenHours.Any(timePeriod => timePeriod.Contains(layerTimePeriod)))
					{
						return true;
					}

					if (layerTimePeriod.EndTime.Days <= 0) continue;
					var nextSkillDay = skillDays.FirstOrDefault(x => !(x.Skill is MaxSeatSkill) && x.CurrentDate.Equals(layerStartDate.AddDays(1)) && x.Skill.Activity.Equals(layerActivity));
					if (nextSkillDay == null) continue;
					foreach (var openHoursPeriod in skillDayOpenHours)
					{
						if (openHoursPeriod.EndTime.Days <= 0 || openHoursPeriod.StartTime > layerTimePeriod.StartTime) continue;
						var nextSkillDayOpenHours = nextSkillDay.OpenHours();
						var nextDayLayerTimePeriod = layerTimePeriod.EndTime.Subtract(TimeSpan.FromDays(1));
						if (nextSkillDayOpenHours.Any(period => period.Contains(nextDayLayerTimePeriod)))
						{
							return true;
						}
					}
				}

			}
			return false;
		}
	}
}