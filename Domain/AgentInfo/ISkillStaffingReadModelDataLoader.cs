using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface ISkillStaffingReadModelDataLoader
	{
		IList<SkillStaffingData> Load(IList<ISkill> skills, DateTimePeriod period);
	}
}