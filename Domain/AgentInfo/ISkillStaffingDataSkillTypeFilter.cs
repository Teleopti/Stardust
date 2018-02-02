using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface ISkillStaffingDataSkillTypeFilter
	{
		IList<SkillStaffingData> Filter(IEnumerable<SkillStaffingData> skillStaffingDatas);
	}
}