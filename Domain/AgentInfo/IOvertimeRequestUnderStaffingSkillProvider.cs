using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface IOvertimeRequestUnderStaffingSkillProvider
	{
		IDictionary<DateTimePeriod,IList<ISkill>> GetSeriousUnderstaffingSkills(DateTimePeriod period, IEnumerable<ISkill> skills, TimeZoneInfo timeZoneInfo);
	}
}