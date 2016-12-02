using System.Collections.Generic;
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
			var skillGroups = new Dictionary<ICollection<ISkill>, ICollection<IPerson>>(new sameSkillGroupSkillsComparer());

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

		private class sameSkillGroupSkillsComparer : IEqualityComparer<ICollection<ISkill>>
		{
			public bool Equals(ICollection<ISkill> x, ICollection<ISkill> y)
			{
				return x.All(y.Contains);
			}

			public int GetHashCode(ICollection<ISkill> obj)
			{
				return obj.Count;
			}
		}
	}
}