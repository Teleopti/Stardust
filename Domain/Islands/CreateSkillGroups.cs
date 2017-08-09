using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
			var skillGroups = new Dictionary<ISet<ISkill>, ISet<IPerson>>(new SameSkillGroupSkillsComparer());

			foreach (var agent in agents)
			{
				var personPeriod = agent.Period(date);
				if (personPeriod == null)
					continue;
				var agentsSkills = new HashSet<ISkill>(_personalSkillsProvider.PersonSkills(personPeriod).Select(x => x.Skill));
				if(!agentsSkills.Any())
					continue;

				if (skillGroups.TryGetValue(agentsSkills, out ISet<IPerson> list))
				{
					list.Add(agent);
				}
				else
				{
					skillGroups.Add(agentsSkills, new HashSet<IPerson> { agent });
				}
			}
			return new SkillGroups(skillGroups.Select(keyValue => new SkillGroup(keyValue.Key, keyValue.Value)));
		}
	}
}