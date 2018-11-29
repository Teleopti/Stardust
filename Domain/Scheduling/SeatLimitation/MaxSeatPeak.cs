using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class MaxSeatPeak
	{
		private readonly IUsedSeats _usedSeats;

		public MaxSeatPeak(IUsedSeats usedSeats)
		{
			_usedSeats = usedSeats;
		}

		public MaxSeatPeakData Fetch(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> maxSeatSkillDaysToLookAt)
		{
			var period = teamBlockInfo.BlockInfo.BlockPeriod.Inflate(1);
			var lookup = maxSeatSkillDaysToLookAt.ToLookup(s => s.CurrentDate);
			var datePeak = period.DayCollection().ToDictionary(d => d, v => fetchForDate(v, lookup));
			return new MaxSeatPeakData(datePeak);
		}

		public MaxSeatPeakData Fetch(IEnumerable<DateOnly> datesToConsider, IEnumerable<ISkillDay> maxSeatSkillDaysToLookAt)
		{
			var lookup = maxSeatSkillDaysToLookAt.ToLookup(s => s.CurrentDate);
			var datePeak = datesToConsider.ToDictionary(d => d, v => fetchForDate(v, lookup));
			return new MaxSeatPeakData(datePeak);
		}

		private double fetchForDate(DateOnly dateOnly, ILookup<DateOnly, ISkillDay> maxSeatSkillDaysToLookAt)
		{
			var retValue = 0d;
			foreach (var skillDay in maxSeatSkillDaysToLookAt[dateOnly])
			{
				var maxSeats = ((MaxSeatSkill)skillDay.Skill).MaxSeats;

				foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
				{
					retValue = Math.Max(retValue, _usedSeats.Fetch(skillStaffPeriod) - maxSeats);
				}
 			}

			return retValue;
 		}
	}
}