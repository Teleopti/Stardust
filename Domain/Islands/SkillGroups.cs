using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillGroups : ISkillGroupInfo, IEnumerable<SkillGroup>
	{
		private readonly IEnumerable<SkillGroup> _skillGroups;

		public SkillGroups(IEnumerable<SkillGroup> skillGroups)
		{
			var skillGroupsTemp = skillGroups.ToList();
			skillGroupsTemp.Sort(new SortSkillGroupBySize());
			_skillGroups = skillGroupsTemp;
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
			return int.MaxValue;
		}

		public IEnumerator<SkillGroup> GetEnumerator()
		{
			return _skillGroups.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}