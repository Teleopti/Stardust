using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class SplitSkillStaffInterval
	{
		public IList<SkillStaffingIntervalLightModel> Split(List<SkillStaffingInterval> staffingList, TimeSpan resolution, bool useShrinkage)
		{
			var dividedIntervals = new List<SkillStaffingIntervalLightModel>();

			foreach (var skillStaffingInterval in staffingList)
			{
				if (!skillStaffingInterval.GetTimeSpan().Equals(resolution))
				{
					dividedIntervals.AddRange(splitStaffinInterval(skillStaffingInterval, resolution, useShrinkage));
				}
				else
				{
					if (useShrinkage)
						dividedIntervals.Add(new SkillStaffingIntervalLightModel
											 {
												 Id = skillStaffingInterval.SkillId,
												 StartDateTime = skillStaffingInterval.StartDateTime,
												 EndDateTime = skillStaffingInterval.EndDateTime,
												 StaffingLevel = skillStaffingInterval.StaffingLevelWithShrinkage
											 });
					else
						dividedIntervals.Add(new SkillStaffingIntervalLightModel
											 {
												 Id = skillStaffingInterval.SkillId,
												 StartDateTime = skillStaffingInterval.StartDateTime,
												 EndDateTime = skillStaffingInterval.EndDateTime,
												 StaffingLevel = skillStaffingInterval.StaffingLevel
											 });
				}
			}
			return dividedIntervals;
		}

		private static IEnumerable<SkillStaffingIntervalLightModel> splitStaffinInterval(SkillStaffingInterval interval, TimeSpan resolution, bool useShrinkage)
		{
			var dividedIntervals = new List<SkillStaffingIntervalLightModel>();
			var startInterval = interval.StartDateTime;
			while (startInterval < interval.EndDateTime)
			{
				if (useShrinkage)
					dividedIntervals.Add(new SkillStaffingIntervalLightModel
										 {
											 Id = interval.SkillId,
											 StartDateTime = startInterval,
											 EndDateTime = startInterval.Add(resolution),
											 StaffingLevel = interval.StaffingLevelWithShrinkage
										 });
				else
					dividedIntervals.Add(new SkillStaffingIntervalLightModel
										 {
											 Id = interval.SkillId,
											 StartDateTime = startInterval,
											 EndDateTime = startInterval.Add(resolution),
											 StaffingLevel = interval.StaffingLevel
										 });
				startInterval = startInterval.Add(resolution);
			}
			return dividedIntervals;
		}
	}
}

