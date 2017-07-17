using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface IOvertimeRequestUnderStaffingSkillProvider
	{
		ISkill GetUnderStaffingSkill(DateTimePeriod period, IEnumerable<ISkill> skills);
	}
}