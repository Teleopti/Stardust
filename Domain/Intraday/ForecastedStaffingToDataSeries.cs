using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ForecastedStaffingToDataSeries
	{
		public ForecastedStaffingToDataSeries()
		{
		}

		public double?[] DataSeries(IList<StaffingIntervalModel> forecastedStaffingPerSkill, DateTime[] timeSeries)
		{
			var forecastedStaffing = forecastedStaffingPerSkill
				.GroupBy(g => g.StartTime)
				.Select(s =>
				{
					var staffingIntervalModel = s.First();
					return new StaffingIntervalModel
					{
						SkillId = staffingIntervalModel.SkillId,
						StartTime = staffingIntervalModel.StartTime,
						Agents = s.Sum(a => a.Agents)
					};
				})
				.OrderBy(o => o.StartTime)
				.ToList();

			if (timeSeries.Length == forecastedStaffing.Count)
				return forecastedStaffing.Select(x => (double?)x.Agents).ToArray();

			var forecastedStaffings = forecastedStaffing.ToLookup(x => x.StartTime);
			return timeSeries.Select(intervalStart => forecastedStaffings[intervalStart].FirstOrDefault()?.Agents).ToArray();
		}
	}
}