using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ScheduledStaffingToDataSeries
	{
		public ScheduledStaffingToDataSeries()
		{
		}

		public double?[] DataSeries(IList<SkillStaffingIntervalLightModel> scheduledStaffing, DateTime[] timeSeries)
		{
			if (!scheduledStaffing.Any())
				return new double?[] {};

			var staffingIntervals = scheduledStaffing
				.GroupBy(x => x.StartDateTime)
				.Select(s => new ScheduledStaffingToDataSeries.StaffingStartInterval
				{
					StartTime = s.Key,
					StaffingLevel = s.Sum(a => a.StaffingLevel)
				})
				.ToList();

			if (timeSeries.Length == staffingIntervals.Count)
				return staffingIntervals.Select(x => (double?)x.StaffingLevel).ToArray();

			List<double?> scheduledStaffingList = new List<double?>();
			foreach (var intervalStart in timeSeries)
			{
				var scheduledStaffingInterval = staffingIntervals.FirstOrDefault(x => x.StartTime == intervalStart);
				scheduledStaffingList.Add(scheduledStaffingInterval?.StaffingLevel);
			}
			return scheduledStaffingList.ToArray();
		}

		private class StaffingStartInterval
		{
			public DateTime StartTime { get; set; }
			public double StaffingLevel { get; set; }
		}
	}
}