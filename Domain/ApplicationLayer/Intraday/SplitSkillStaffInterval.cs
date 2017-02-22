﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class SplitSkillStaffInterval
	{
		public IList<SkillStaffingIntervalLightModel> Split(List<SkillStaffingInterval> staffingList, TimeSpan resolution)
		{
			Console.WriteLine($"splitStaffinInterval.split -- resolution = {resolution}");
			var dividedIntervals = new List<SkillStaffingIntervalLightModel>();

			foreach (var skillStaffingInterval in staffingList)
			{
				Console.WriteLine($"splitStaffinInterval.split -- interval.DateTimePeriod = {skillStaffingInterval.DateTimePeriod}");
				
				if (!skillStaffingInterval.GetTimeSpan().Equals(resolution))
				{
					dividedIntervals.AddRange(splitStaffinInterval(skillStaffingInterval, resolution));
				}
				else
				{
					dividedIntervals.Add(new SkillStaffingIntervalLightModel()
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

		private static IEnumerable<SkillStaffingIntervalLightModel> splitStaffinInterval(SkillStaffingInterval interval, TimeSpan resolution)
		{
			Console.WriteLine($"splitStaffinInterval.splitStaffinInterval -- interval.DateTimePeriod = {interval.DateTimePeriod}");
			var dividedIntervals = new List<SkillStaffingIntervalLightModel>();
			var startInterval = interval.StartDateTime;
			while (startInterval < interval.EndDateTime)
			{
				dividedIntervals.Add(new SkillStaffingIntervalLightModel()
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

