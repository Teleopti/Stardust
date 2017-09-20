using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class SkillSets : IEnumerable<SkillSet>
	{
		private readonly IEnumerable<SkillSet> _skillSets;

		public SkillSets(IEnumerable<SkillSet> skillSets)
		{
			_skillSets = skillSets;
		}

		public IEnumerable<IEnumerable<IPerson>> AgentsGroupedBySkillSet()
		{
			return _skillSets.Select(skillSet => skillSet.Agents);
		}

		public int NumberOfAgentsInSameSkillSet(IPerson agent)
		{
			foreach (var skillSet in _skillSets)
			{
				if (skillSet.Agents.Contains(agent))
				{
					return skillSet.Agents.Count();
				}
			}
			return int.MaxValue;
		}

		public IEnumerator<SkillSet> GetEnumerator()
		{
			return _skillSets.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}