using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public interface IReduceSkillGroups
	{
		bool Execute(IEnumerable<IEnumerable<SkillGroup>> groupedSkillGroups, IDictionary<ISkill, int> noAgentsKnowingSkill);
	}
}