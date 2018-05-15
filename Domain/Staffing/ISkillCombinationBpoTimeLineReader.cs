using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface ISkillCombinationBpoTimeLineReader
	{
		IEnumerable<SkillCombinationResourceBpoTimelineModel> GetAllDataForBpoTimeline();
		IEnumerable<SkillCombinationResourceBpoTimelineModel> GetBpoTimelineDataForSkill(Guid skillId);
		IEnumerable<SkillCombinationResourceBpoTimelineModel> GetBpoTimelineDataForSkillGroup(Guid skillGroupId);

		IEnumerable<SkillCombinationResourceBpoImportInfoModel> GetBpoImportInfoForSkill(Guid skillId,
			DateTime startDateTimeUtc, DateTime endDateTimeUtc);
		IEnumerable<SkillCombinationResourceBpoImportInfoModel> GetBpoImportInfoForSkillGroup(Guid skillGroupId,
			DateTime startDateTimeUtc, DateTime endDateTimeUtc);
	}
}