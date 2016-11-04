using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class MergeSkillStaffIntervalLightForSkillArea
	{
		public IList<SkillStaffingIntervalLightModel> Merge(List<SkillStaffingIntervalLightModel> staffingList, TimeSpan fromMinutes, Guid skillAreaId)
		{
			var mergedIntervals = new List<SkillStaffingIntervalLightModel>();
			foreach (var interval in staffingList.OrderBy(x => x.StartDateTime))
			{
				if (!vistitedInterval(mergedIntervals, interval))
				{
					var partialInterval = interval;
					partialInterval.Id = skillAreaId;
					mergedIntervals.Add(partialInterval);
				}
				else
				{
					var existingInterval = mergedIntervals.FirstOrDefault(x => x.StartDateTime == interval.StartDateTime && x.EndDateTime == interval.EndDateTime);
					mergedIntervals.Remove(existingInterval);
					existingInterval.StaffingLevel += interval.StaffingLevel;
					mergedIntervals.Add(existingInterval);
				}
			}
			return mergedIntervals;
		}

		private bool vistitedInterval(List<SkillStaffingIntervalLightModel> mergedIntervals, SkillStaffingIntervalLightModel interval)
		{
			return mergedIntervals.Any(x => x.StartDateTime == interval.StartDateTime && x.EndDateTime == interval.EndDateTime);
		}
	}
}
