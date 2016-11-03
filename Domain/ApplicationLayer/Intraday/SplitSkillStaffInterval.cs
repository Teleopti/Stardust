using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class SplitSkillStaffInterval
	{
		public IList<SkillStaffingIntervalLight> Split(List<SkillStaffingInterval> staffingList, TimeSpan resolution)
		{
			var dividedIntervals = new List<SkillStaffingIntervalLight>();
			foreach (var skillStaffingInterval in staffingList)
			{
				if (!skillStaffingInterval.GetTimeSpan().Equals(resolution))
				{
					dividedIntervals.AddRange(splitStaffinInterval(skillStaffingInterval, resolution));
				}
				else
				{
					dividedIntervals.Add(new SkillStaffingIntervalLight()
					{
						SkillId = skillStaffingInterval.SkillId,
						StartDateTime = skillStaffingInterval.StartDateTime,
						EndDateTime = skillStaffingInterval.EndDateTime,
						StaffingLevel = skillStaffingInterval.StaffingLevel
					});
				}
			}
			return dividedIntervals;
		}

		private IEnumerable<SkillStaffingIntervalLight> splitStaffinInterval(SkillStaffingInterval interval, TimeSpan resolution)
		{
			var dividedIntervals = new List<SkillStaffingIntervalLight>();
			var divisor = interval.DivideBy(resolution);
			var startInterval = interval.StartDateTime;
			while (startInterval < interval.EndDateTime)
			{
				dividedIntervals.Add(new SkillStaffingIntervalLight()
				{
					SkillId = interval.SkillId,
					StartDateTime = startInterval,
					EndDateTime = startInterval.Add(resolution),
					StaffingLevel = Math.Round((interval.StaffingLevel / divisor), 5)
				});
				startInterval = startInterval.Add(resolution);
			}
			return dividedIntervals;
		}
	}
}

