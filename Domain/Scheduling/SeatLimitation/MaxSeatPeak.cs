using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
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

		public double Fetch(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> maxSeatSkillDaysToLookAt)
		{
			return teamBlockInfo.BlockInfo.BlockPeriod.DayCollection()
				.Select(dateOnly => Fetch(dateOnly, maxSeatSkillDaysToLookAt))
				.Max();
		}

		public double Fetch(DateOnly dateOnly, IEnumerable<ISkillDay> maxSeatSkillDaysToLookAt)
		{
			var retValue = 0d;
			foreach (var skillDay in maxSeatSkillDaysToLookAt.Where(x => x.CurrentDate == dateOnly))
			{
				var maxSeats = ((MaxSeatSkill) skillDay.Skill).MaxSeats;

				foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
				{
					retValue = Math.Max(retValue, _usedSeats.Fetch(skillStaffPeriod) - maxSeats);
				}
			}

			return retValue;
		}
	}
}