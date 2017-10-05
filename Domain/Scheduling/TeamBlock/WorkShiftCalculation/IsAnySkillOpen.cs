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
					if (skillDay.OpenHours().Any(timePeriod => timePeriod.Contains(layer.Period.TimePeriod(skillTimeZone))))
					{
						return true;
					}
				}

			}
			return false;
		}
	}
}