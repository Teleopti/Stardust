using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

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
			var sites = new HashSet<ISite>();
			foreach (var agent in teamBlockInfo.TeamInfo.GroupMembers)
			{
				var personPeriod = agent.Period(personPeriodDate);
				if (personPeriod != null)
				{
					sites.Add(personPeriod.Team.Site);
				}
			}
			return sites.SelectMany(site => _maxSeatSkillDataPerSkills.SingleOrDefault(x => x.Site.Equals(site))?.SkillDays ?? new ISkillDay[0])
				.ToArray();
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