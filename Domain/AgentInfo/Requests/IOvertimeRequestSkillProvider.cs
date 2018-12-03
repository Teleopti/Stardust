using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IOvertimeRequestSkillProvider
	{
		IEnumerable<ISkill> GetAvailableSkillsBySkillType(IPerson person, DateTimePeriod requestPeriod);
	}
}