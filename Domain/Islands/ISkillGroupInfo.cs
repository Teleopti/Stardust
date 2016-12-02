using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public interface ISkillGroupInfo
	{
		IEnumerable<IEnumerable<IPerson>> AgentsGroupedBySkillGroup();
		int NumberOfAgentsInSameSkillGroup(IPerson agent);
	}
}