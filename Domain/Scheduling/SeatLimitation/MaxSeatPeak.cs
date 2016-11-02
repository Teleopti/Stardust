using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class MaxSeatPeak
	{
		private readonly IUsedSeats _usedSeats;

		public MaxSeatPeak(IUsedSeats usedSeats)
		{
			_usedSeats = usedSeats;
		}

		public double Fetch(DateOnly date, MaxSeatSkillData maxSeatSkillData, IEnumerable<ISkillDay> skillDaysToLookAt)
		{
			var retValue = 0d;
			foreach (var skillDay in skillDaysToLookAt)
			{
				var maxSeats = maxSeatSkillData.MaxSeatForSkill(skillDay.Skill);

				foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
				{
					retValue = Math.Max(retValue, _usedSeats.Fetch(skillStaffPeriod) - maxSeats);
				}
			}
			return retValue;
		}
	}
}