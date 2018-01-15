using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Forecasting.Angel
{
	public class OutlierRemover : IOutlierRemover
	{

		public ITaskOwnerPeriod RemoveOutliers(ITaskOwnerPeriod historicalData, IForecastMethod forecastMethod)
		{
			var seasonalVariationPossiblyWithTrend = forecastMethod.SeasonalVariation(historicalData);

			var normalDistribution = new Dictionary<DateOnly, double>();
			foreach (var day in seasonalVariationPossiblyWithTrend)
			{
				foreach (var historicalDay in historicalData.TaskOwnerDayCollection)
				{
					if (day.Date == historicalDay.CurrentDate)
					{
						if (Math.Abs(historicalDay.TotalStatisticCalculatedTasks) > 0.001d)
							normalDistribution.Add(day.Date, historicalDay.TotalStatisticCalculatedTasks - day.Tasks);
						break;
					}
				}
			}
			
			var descriptiveStatistics = new DescriptiveStatistics(normalDistribution.Values);
			var mean = descriptiveStatistics.Mean;
			var stdDev = descriptiveStatistics.StandardDeviation;
			var upper = mean + 3*stdDev;
			var lower = mean - 3*stdDev;

			foreach (var day in normalDistribution)
			{
				if (day.Value > upper)
				{
					var taskOwner = historicalData.TaskOwnerDayCollection.Single(x => x.CurrentDate == day.Key);
					((ValidatedVolumeDay)taskOwner).ValidatedTasks = upper + seasonalVariationPossiblyWithTrend.Single(x => x.Date == day.Key).Tasks;
				}
				else if (day.Value < lower)
				{
					var taskOwner = historicalData.TaskOwnerDayCollection.Single(x => x.CurrentDate == day.Key);
					((ValidatedVolumeDay)taskOwner).ValidatedTasks = lower + seasonalVariationPossiblyWithTrend.Single(x => x.Date == day.Key).Tasks;
				}
			}

			return historicalData;
		}
	}
}