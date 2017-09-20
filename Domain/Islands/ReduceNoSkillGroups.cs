using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class ReduceNoSkillGroups : IReduceSkillGroups
	{
		public bool Execute(IEnumerable<IEnumerable<SkillSet>> groupedSkillGroups, IDictionary<ISkill, int> noAgentsKnowingSkill)
		{
			return false;
		}
	}
}