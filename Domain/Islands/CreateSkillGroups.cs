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
			var skillGroups = new Dictionary<ICollection<ISkill>, ICollection<IPerson>>(new SameSkillGroupSkillsComparer());

			foreach (var agent in agents)
			{
				var personPeriod = agent.Period(date);
				if (personPeriod == null)
					continue;
				var agentsSkills = _personalSkillsProvider.PersonSkills(personPeriod).Select(x => x.Skill).ToList();

				ICollection<IPerson> list;
				if (skillGroups.TryGetValue(agentsSkills, out list))
				{
					list.Add(agent);
				}
				else
				{
					skillGroups.Add(agentsSkills, new List<IPerson> { agent });
				}
			}
			return new SkillGroups(skillGroups.Select(keyValue => new SkillGroup(keyValue.Key, keyValue.Value)));
		}
	}
}