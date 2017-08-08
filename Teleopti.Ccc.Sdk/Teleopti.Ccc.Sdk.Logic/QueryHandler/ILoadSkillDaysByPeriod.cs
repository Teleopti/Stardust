using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public interface ILoadSkillDaysByPeriod
	{
		ICollection<SkillDayDto> GetSkillDayDto(GetSkillDaysByPeriodQueryDto query);
	}
}
