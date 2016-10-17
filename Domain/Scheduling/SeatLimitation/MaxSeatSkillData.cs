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

		public void Add(ISkill skill, IEnumerable<ISkillDay> skillDays, ISite site)
		{
			_maxSeatSkillDataPerSkills.Add(new maxSeatSkillDataPerSkill(skill, skillDays, site));
		}

		public IEnumerable<ISkill> AllMaxSeatSkills()
		{
			return _maxSeatSkillDataPerSkills.Select(x => x.Skill);
		}

		public IEnumerable<ISkillDay> SkillDaysForDate(DateOnly date)
		{
			return _maxSeatSkillDataPerSkills.SelectMany(x => x.SkillDays).Where(x => x.CurrentDate == date);
		}

		public IDictionary<ISkill, IEnumerable<ISkillDay>> AllMaxSeatSkillDaysPerSkill()
		{
			return _maxSeatSkillDataPerSkills.ToDictionary(maxSeatSkillDataPerSkill => maxSeatSkillDataPerSkill.Skill, maxSeatSkillDataPerSkill => maxSeatSkillDataPerSkill.SkillDays);
		}

		public int MaxSeats(ISkill skill)
		{
			return _maxSeatSkillDataPerSkills.Single(x => x.Skill.Equals(skill)).Site.MaxSeats.Value;
		}


		private class maxSeatSkillDataPerSkill
		{
			public maxSeatSkillDataPerSkill(ISkill skill, IEnumerable<ISkillDay> skillDays, ISite site)
			{
				Skill = skill;
				SkillDays = skillDays;
				Site = site;
			}

			public ISkill Skill { get; }
			public IEnumerable<ISkillDay> SkillDays { get; }
			public ISite Site { get; }
		}
	}
}