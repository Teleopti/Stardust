using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface IOvertimeRequestUnderStaffingSkillProvider
	{
		IDictionary<DateTimePeriod,IList<ISkill>> GetSeriousUnderstaffingSkills(DateTimePeriod period, IEnumerable<ISkill> skills, IPerson person);
	}
}