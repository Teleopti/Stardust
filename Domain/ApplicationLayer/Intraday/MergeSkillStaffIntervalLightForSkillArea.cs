using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class MergeSkillStaffIntervalLightForSkillArea
	{
		public IList<SkillStaffingIntervalLightModel> Merge(List<SkillStaffingIntervalLightModel> staffingList, Guid skillAreaId)
		{
			var mergedIntervals = staffingList.GroupBy(x => new { x.StartDateTime, x.EndDateTime})
			 .Select(g => new SkillStaffingIntervalLightModel()
			 {
				 Id = skillAreaId,
				 StartDateTime = g.Key.StartDateTime,
				 EndDateTime = g.Key.EndDateTime,
				 StaffingLevel = g.Sum(c => c.StaffingLevel),
			 }).ToList();
			return mergedIntervals;
		}
	}
}
