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
			if (!((IActivity)layer.Payload).RequiresSkill)
				return true;

			foreach (var skillDay in skillDays)
			{
				if (!(skillDay.Skill is MaxSeatSkill) &&
					skillDay.Skill.Activity.Equals((IActivity)layer.Payload))
				{
					if (skillDay.OpenHours().Any(timePeriod => timePeriod.Contains(layer.Period.TimePeriod(TimeZoneInfo.Utc))))
					{
						return true;
					}
				}

			}
			return false;
		}
	}
}