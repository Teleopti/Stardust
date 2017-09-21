using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class CreateSkillSets
	{
		private readonly PersonalSkillsProvider _personalSkillsProvider;

		public CreateSkillSets(PersonalSkillsProvider personalSkillsProvider)
		{
			_personalSkillsProvider = personalSkillsProvider;
		}

		public SkillSets Create(IEnumerable<IPerson> agents, DateOnly date)
		{
			var skillSets = new Dictionary<ISet<ISkill>, ISet<IPerson>>(new SameSkillSetSkillsComparer());

			foreach (var agent in agents)
			{
				var personPeriod = agent.Period(date);
				if (personPeriod == null)
					continue;
				var agentsSkills = new HashSet<ISkill>(_personalSkillsProvider.PersonSkills(personPeriod).Select(x => x.Skill));
				if(!agentsSkills.Any())
					continue;

				if (skillSets.TryGetValue(agentsSkills, out ISet<IPerson> list))
				{
					list.Add(agent);
				}
				else
				{
					skillSets.Add(agentsSkills, new HashSet<IPerson> { agent });
				}
			}
			return new SkillSets(skillSets.Select(keyValue => new SkillSet(keyValue.Key, keyValue.Value)));
		}
	}
}