using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Intraday
{
	public static class TimeSeriesProvider
	{
		public static DateTime[] DataSeries(IList<StaffingIntervalModel> forecastedStaffing, IList<SkillStaffingIntervalLightModel> scheduledStaffing, int minutesPerInterval, TimeZoneInfo timeZoneInfo)
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

			var ambiguousTimes = new List<DateTime>();
			for (var time = theMinTime; time <= theMaxTime; time = time.AddMinutes(minutesPerInterval))
			{
				if(timeZoneInfo != null && timeZoneInfo.IsAmbiguousTime(time))
					ambiguousTimes.Add(time);

				if (!ambiguousTimes.IsEmpty() && timeZoneInfo != null && !timeZoneInfo.IsAmbiguousTime(time))
				{
					timeSeries.AddRange(ambiguousTimes);
					ambiguousTimes.Clear();
				}
				
				if(timeZoneInfo == null || !timeZoneInfo.IsInvalidTime(time))
					timeSeries.Add(time);
			}
			
			foreach (var timeSeriesTime in timeSeries)
			{
				if(timeZoneInfo != null && timeZoneInfo.IsAmbiguousTime(timeSeriesTime))
					ambiguousTimes.Add(timeSeriesTime);
			}

			return timeSeries.ToArray();
		}
	}
}