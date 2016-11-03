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

		public double Fetch(ITeamBlockInfo teamBlockInfo, MaxSeatSkillData maxSeatSkillData, IEnumerable<ISkillDay> skillDaysToLookAt)
		{
			var retValue = 0d;
			foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
			{
				foreach (var skillDay in skillDaysToLookAt.Where(x => x.CurrentDate == dateOnly))
				{
					var maxSeats = maxSeatSkillData.MaxSeatForSkill(skillDay.Skill);

					foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
					{
						retValue = Math.Max(retValue, _usedSeats.Fetch(skillStaffPeriod) - maxSeats);
					}
				}
			}
			return retValue;
		}
	}
}