using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IOvertimeRequestSkillProvider
	{
		IEnumerable<ISkill> GetAvailableSkills(DateTimePeriod requestPeriod);
	}
}