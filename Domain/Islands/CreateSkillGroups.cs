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

		public IEnumerable<SkillGroup> Create(IEnumerable<IPerson> agents, DateOnly date)
		{
			//We need to include the concept skillgroup here! not needed yet -> only when islands are run and there we still use old behavior
			var skillGroups = new Dictionary<ICollection<ISkill>, ICollection<IPerson>>();

			foreach (var agent in agents)
			{
				var personPeriod = agent.Period(date);
				if (personPeriod == null)
					continue;
				var agentsSkills = _personalSkillsProvider.PersonSkills(personPeriod).Select(x => x.Skill).ToList();
				if (!skillGroups.ContainsKey(agentsSkills))
					skillGroups[agentsSkills] = new List<IPerson>();
				skillGroups[agentsSkills].Add(agent);  
			}

			return skillGroups.Select(keyValue => new SkillGroup(keyValue.Key, keyValue.Value)).ToList();
		}
	}
}