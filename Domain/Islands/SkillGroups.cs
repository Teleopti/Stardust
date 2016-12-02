using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	//TODO: Should be used when creating skillgroups/islands!

	public class SkillGroups : ISkillGroupInfo
	{
		private readonly IEnumerable<SkillGroup> _skillGroups;

		public SkillGroups(IEnumerable<SkillGroup> skillGroups)
		{
			_skillGroups = skillGroups;
		}

		public IEnumerable<IEnumerable<IPerson>> AgentsGroupedBySkillGroup()
		{
			return _skillGroups.Select(skillGroup => skillGroup.Agents);
		}

		public int NumberOfAgentsInSameSkillGroup(IPerson agent)
		{
			foreach (var skillGroup in _skillGroups)
			{
				if (skillGroup.Agents.Contains(agent))
				{
					return skillGroup.Agents.Count();
				}
			}
			return 0;
		}
	}
}