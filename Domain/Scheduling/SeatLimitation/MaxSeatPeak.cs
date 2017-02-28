using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public MaxSeatPeakData Fetch(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> maxSeatSkillDaysToLookAt)
		{
			var period = teamBlockInfo.BlockInfo.BlockPeriod.Inflate(1);
			var datePeak = new Dictionary<DateOnly, double>();
			foreach (var date in period.DayCollection())
			{
				datePeak[date] = fetchForDate(date, maxSeatSkillDaysToLookAt);
			}
			return new MaxSeatPeakData(datePeak);
		}

		public MaxSeatPeakData Fetch(IEnumerable<DateOnly> datesToConsider, IEnumerable<ISkillDay> maxSeatSkillDaysToLookAt)
		{
			var datePeak = new Dictionary<DateOnly, double>();
			foreach (var date in datesToConsider)
			{
				datePeak[date] = fetchForDate(date, maxSeatSkillDaysToLookAt);
			}
			return new MaxSeatPeakData(datePeak);
		}

		private double fetchForDate(DateOnly dateOnly, IEnumerable<ISkillDay> maxSeatSkillDaysToLookAt)
		{
			var retValue = 0d;
			foreach (var skillDay in maxSeatSkillDaysToLookAt.Where(x => x.CurrentDate == dateOnly))
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