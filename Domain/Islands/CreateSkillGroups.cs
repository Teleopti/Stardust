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
			//We need to include the concept skillgroup here! not needed yet -> only when islands are run and there we still use old behavior
			var skillGroups = new Dictionary<ICollection<ISkill>, ICollection<IPerson>>();

			foreach (var agent in agents)
			{
				var personPeriod = agent.Period(date);
				if (personPeriod == null)
					continue;
				var agentsSkills = _personalSkillsProvider.PersonSkills(personPeriod).Select(x => x.Skill).ToList();

				var key = containsKey(skillGroups, agentsSkills);
				if ( key == null)
				{
					skillGroups.Add(agentsSkills, new List<IPerson> {agent});
				}
				else
				{
					skillGroups[key].Add(agent);
				}
				
			}

			return new SkillGroups(skillGroups.Select(keyValue => new SkillGroup(keyValue.Key, keyValue.Value)).ToList());
		}

		private ICollection<ISkill> containsKey(Dictionary<ICollection<ISkill>, ICollection<IPerson>> skillGroups, IList<ISkill> skills)
		{
			foreach (var skillGroupsKey in skillGroups.Keys)
			{
				if (skillGroupsKey.Count != skills.Count)
					return null;

				foreach (var skill in skillGroupsKey)
				{
					if (!skills.Contains(skill))
						return null;
				}

				return skillGroupsKey;
			}

			return null;
		}
	}
}