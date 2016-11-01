﻿using System;
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

		public double Fetch(DateOnly date, MaxSeatSkillData maxSeatSkillData, ITeamBlockInfo teamBlockInfo, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays)
		{
			//TODO: Will only work for business hierarchy
			var maxSeatSkill = teamBlockInfo.TeamInfo.GroupMembers.First().Period(date).MaxSeatSkill;
			var retValue = 0d;
			foreach (var skillDay in skillDays[maxSeatSkill])
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