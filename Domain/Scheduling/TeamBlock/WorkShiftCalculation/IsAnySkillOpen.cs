using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public class IsAnySkillOpen
	{
		public bool Check(IEnumerable<ISkillDay> skillDays, IVisualLayer layer)
		{
			foreach (var skillDay in skillDays.Where(x => x.Skill is MaxSeatSkill 
																		&& x.Skill.Activity.Equals((IActivity)layer.Payload)))
			{
				if (skillDay.OpenHours().Any(timePeriod => timePeriod.Contains(layer.Period.TimePeriod(TimeZoneInfo.Utc))))
				{
					return true;
				}
			}
			return false;
		}
	}
}