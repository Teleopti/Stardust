using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface IPrimaryPersonSkillFilter
	{
		IList<IPersonSkill> Filter(IEnumerable<IPersonSkill> personSkills);
	}
}