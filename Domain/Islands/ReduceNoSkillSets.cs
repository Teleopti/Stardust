using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class ReduceNoSkillSets : IReduceSkillSets
	{
		public bool Execute(IEnumerable<IEnumerable<SkillSet>> groupedSkillSets, IDictionary<ISkill, int> noAgentsKnowingSkill)
		{
			return false;
		}
	}
}