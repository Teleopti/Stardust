using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
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

		public void Add(MaxSeatSkill maxSeatSkill, IEnumerable<ISkillDay> skillDays, ISite site)
		{
			_maxSeatSkillDataPerSkills.Add(new maxSeatSkillDataPerSkill(maxSeatSkill, skillDays, site));
		}

		public IEnumerable<MaxSeatSkill> AllMaxSeatSkills()
		{
			return _maxSeatSkillDataPerSkills.Select(x => x.Skill);
		}

		public IDictionary<MaxSeatSkill, IEnumerable<ISkillDay>> AllMaxSeatSkillDaysPerSkill()
		{
			return _maxSeatSkillDataPerSkills.ToDictionary(maxSeatSkillDataPerSkill => maxSeatSkillDataPerSkill.Skill, maxSeatSkillDataPerSkill => maxSeatSkillDataPerSkill.SkillDays);
		}

		public bool MaxSeatSkillExists()
		{
			return _maxSeatSkillDataPerSkills.Any();
		}

		public IEnumerable<ISkillDay> SkillDaysFor(ITeamBlockInfo teamBlockInfo, DateOnly personPeriodDate)
		{
			var skillDays = new HashSet<ISkillDay>();
			foreach (var agent in teamBlockInfo.TeamInfo.GroupMembers)
			{
				var personPeriod = agent.Period(personPeriodDate);
				var site = personPeriod.Team.Site;
				_maxSeatSkillDataPerSkills.Single(x => x.Site.Equals(site)).SkillDays.ForEach(x => skillDays.Add(x));
			}
			return skillDays;
		}

		private class maxSeatSkillDataPerSkill
		{
			public maxSeatSkillDataPerSkill(MaxSeatSkill skill, IEnumerable<ISkillDay> skillDays, ISite site)
			{
				Skill = skill;
				SkillDays = skillDays;
				Site = site;
			}

			public MaxSeatSkill Skill { get; }
			public IEnumerable<ISkillDay> SkillDays { get; }
			public ISite Site { get; }
		}
	}
}