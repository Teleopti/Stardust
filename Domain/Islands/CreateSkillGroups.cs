﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class CreateSkillGroups
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public CreateSkillGroups(PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
		}

		public SkillGroups Create(IEnumerable<IPerson> agents, DateOnly date)
		{
			//We need to include the concept skillgroup here! not needed yet -> only when islands are run and there we still use old behavior
			var skillGroups = new Dictionary<ICollection<ISkill>, ICollection<IPerson>>(new SameSkillGroupSkillsComparer());

			foreach (var agent in agents)
			{
				var personPeriod = agent.Period(date);
				if (personPeriod == null)
					continue;
				var agentsSkills = _personalSkillsProvider.PersonSkills(personPeriod).Select(x => x.Skill).ToList();

				if (!skillGroups.ContainsKey(agentsSkills))
				{
					skillGroups.Add(agentsSkills, new List<IPerson> {agent});
				}
				else
				{
					skillGroups[agentsSkills].Add(agent);
				}
				
			}

			return new SkillGroups(skillGroups.Select(keyValue => new SkillGroup(keyValue.Key, keyValue.Value)).ToList());
		}
	}

	public class SameSkillGroupSkillsComparer : IEqualityComparer<ICollection<ISkill>>
	{
		public bool Equals(ICollection<ISkill> x, ICollection<ISkill> y)
		{
			foreach (var skill in x)
			{
				if (!y.Contains(skill))
					return false;
			}
			return true;
		}

		public int GetHashCode(ICollection<ISkill> obj)
		{
			return obj.Count;
		}
	}
}