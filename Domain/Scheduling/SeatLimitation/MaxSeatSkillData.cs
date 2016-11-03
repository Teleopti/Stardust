using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
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

		public IDictionary<ISkill, IEnumerable<ISkillDay>> AllMaxSeatSkillDaysPerSkill()
		{
			return _maxSeatSkillDataPerSkills.ToDictionary(maxSeatSkillDataPerSkill => maxSeatSkillDataPerSkill.Skill, maxSeatSkillDataPerSkill => maxSeatSkillDataPerSkill.SkillDays);
		}

		public bool MaxSeatSkillExists()
		{
			return _maxSeatSkillDataPerSkills.Any();
		}

		public int MaxSeatForSkill(ISkill skill)
		{
			return _maxSeatSkillDataPerSkills.Single(x => x.Skill.Equals(skill)).Site.MaxSeats.Value;
		}

		public IEnumerable<ISkillDay> SkillDaysFor(ITeamBlockInfo teamBlockInfo, DateOnly date)
		{
			//This won't work if not hiearchy
			return
				_maxSeatSkillDataPerSkills.Single(
					x => x.Site.Equals(teamBlockInfo.TeamInfo.GroupMembers.First().Period(date).Team.Site)).SkillDays.Where(x => x.CurrentDate==date);
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