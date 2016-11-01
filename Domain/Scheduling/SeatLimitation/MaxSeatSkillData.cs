using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class MaxSeatSkillData
	{
		private readonly IList<maxSeatSkillDataPerSkill> _maxSeatSkillDataPerSkills;

		public MaxSeatSkillData()
		{
			_maxSeatSkillDataPerSkills = new List<maxSeatSkillDataPerSkill>();
		}

		public void Add(ISkill skill, IEnumerable<ISkillDay> skillDays)
		{
			_maxSeatSkillDataPerSkills.Add(new maxSeatSkillDataPerSkill(skill, skillDays));
		}

		public IEnumerable<ISkill> AllMaxSeatSkills()
		{
			return _maxSeatSkillDataPerSkills.Select(x => x.Skill);
		}

		public IDictionary<ISkill, IEnumerable<ISkillDay>> AllMaxSeatSkillDaysPerSkill()
		{
			return _maxSeatSkillDataPerSkills.ToDictionary(maxSeatSkillDataPerSkill => maxSeatSkillDataPerSkill.Skill, maxSeatSkillDataPerSkill => maxSeatSkillDataPerSkill.SkillDays);
		}

		public bool MaxSeatSkillExists()
		{
			return _maxSeatSkillDataPerSkills.Any();
		}

		private class maxSeatSkillDataPerSkill
		{
			public maxSeatSkillDataPerSkill(ISkill skill, IEnumerable<ISkillDay> skillDays)
			{
				Skill = skill;
				SkillDays = skillDays;
			}

			public ISkill Skill { get; }
			public IEnumerable<ISkillDay> SkillDays { get; }
		}
	}
}