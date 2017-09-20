using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class NumberOfAgentsKnowingSkill
	{
		public IDictionary<ISkill, int> Execute(IEnumerable<SkillSet> skillGroups)
		{
			var numberOfAgentsKnowingSkill = new Dictionary<ISkill, int>();
			foreach (var skillGroup in skillGroups)
			{
				var count = skillGroup.Agents.Count();
				foreach (var skill in skillGroup.Skills)
				{
					if (!numberOfAgentsKnowingSkill.ContainsKey(skill))
					{
						numberOfAgentsKnowingSkill[skill] = 0;
					}
					numberOfAgentsKnowingSkill[skill] += count;
				}
			}
			return numberOfAgentsKnowingSkill;
		}
	}
}