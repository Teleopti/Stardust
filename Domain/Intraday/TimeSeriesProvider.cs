using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class TimeSeriesProvider
	{

		public DateTime[] DataSeries(IList<StaffingIntervalModel> forecastedStaffing, IList<SkillStaffingIntervalLightModel> scheduledStaffing, int minutesPerInterval)
		{
			var min1 = DateTime.MaxValue;
			var min2 = DateTime.MaxValue;
			var max1 = DateTime.MinValue;
			var max2 = DateTime.MinValue;
			if (forecastedStaffing.Any())
			{
				min1 = forecastedStaffing.Min(x => x.StartTime);
				max1 = forecastedStaffing.Max(x => x.StartTime);
			}

			if (scheduledStaffing.Any())
			{
				min2 = scheduledStaffing.Min(x => x.StartDateTime);
				max2 = scheduledStaffing.Max(x => x.StartDateTime);
			}

			var theMinTime = min1 > min2 ? min2 : min1;
			var theMaxTime = max1 > max2 ? max1 : max2;

			var timeSeries = new List<DateTime>();

			for (DateTime time = theMinTime; time <= theMaxTime; time = time.AddMinutes(minutesPerInterval))
			{
				timeSeries.Add(time);
			}

			return timeSeries.ToArray();
		}
	}
}