using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public interface IReduceSkillGroups
	{
		bool Execute(IEnumerable<IEnumerable<SkillSet>> groupedSkillGroups, IDictionary<ISkill, int> noAgentsKnowingSkill);
	}
}