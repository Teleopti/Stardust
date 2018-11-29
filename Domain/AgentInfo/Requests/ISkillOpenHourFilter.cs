using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface ISkillOpenHourFilter
	{
		IEnumerable<ISkill> Filter(DateTimePeriod requestPeriod, IEnumerable<ISkill> skills);
	}
}