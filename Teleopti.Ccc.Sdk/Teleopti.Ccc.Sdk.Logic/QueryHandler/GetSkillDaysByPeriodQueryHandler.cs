using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetSkillDaysByPeriodQueryHandler : IHandleQuery<GetSkillDaysByPeriodQueryDto,ICollection<SkillDayDto>>
	{

		private readonly ILoadSkillDaysByPeriod _loadSkillDaysByPeriod;

		public GetSkillDaysByPeriodQueryHandler(ILoadSkillDaysByPeriod loadSkillDaysByPeriod)
		{
			_loadSkillDaysByPeriod = loadSkillDaysByPeriod;
		}

		public ICollection<SkillDayDto> Handle(GetSkillDaysByPeriodQueryDto query)
		{
			return _loadSkillDaysByPeriod.GetSkillDayDto(query);
		}
	}
}