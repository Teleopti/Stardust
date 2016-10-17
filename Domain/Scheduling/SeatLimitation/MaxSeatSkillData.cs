using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class MaxSeatSkillData
	{
		public MaxSeatSkillData(IEnumerable<MaxSeatSkillDataPerSkill> maxSeatSkillDataPerSkills)
		{
			MaxSeatSkillDataPerSkills = maxSeatSkillDataPerSkills;
		}

		public IEnumerable<MaxSeatSkillDataPerSkill> MaxSeatSkillDataPerSkills { get; }

		public IEnumerable<ISkill> AllMaxSeatSkills()
		{
			return MaxSeatSkillDataPerSkills.Select(x => x.Skill);
		}

		public IEnumerable<ISkillDay> SkillDaysForDate(DateOnly date)
		{
			return MaxSeatSkillDataPerSkills.SelectMany(x => x.SkillDays).Where(x => x.CurrentDate == date);
		}
	}

	public class MaxSeatSkillDataPerSkill
	{
		public MaxSeatSkillDataPerSkill(ISkill skill, IEnumerable<ISkillDay> skillDays, int maxSeatsValue)
		{
			Skill = skill;
			SkillDays = skillDays;
			MaxSeatsValue = maxSeatsValue;
		}

		public ISkill Skill { get; }
		public IEnumerable<ISkillDay> SkillDays { get; }
		public int MaxSeatsValue { get; }
	}
}