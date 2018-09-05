using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Outlier
{
	public class FakeTaskOutlierRemoverWithCounter : IOutlierRemover
	{
		public int InvokeCount = 0;
		public ITaskOwnerPeriod RemoveOutliers(ITaskOwnerPeriod historicalData, IForecastMethod forecastMethod)
		{
			InvokeCount++;
			return null;
		}
	}

	public class TaskOutlierRemover : IOutlierRemover
	{
		public ITaskOwnerPeriod RemoveOutliers(ITaskOwnerPeriod historicalData, IForecastMethod forecastMethod)
		{
			var seasonalVariationPossiblyWithTrend = forecastMethod.SeasonalVariation(historicalData);
			var historicalLookup = historicalData.TaskOwnerDayCollection.ToLookup(k => k.CurrentDate);

			var valuesToRemoveOutlier = new List<outlierModel>();
			foreach (var day in seasonalVariationPossiblyWithTrend)
			{
				foreach (var historicalDay in historicalLookup[day.Date])
				{
					valuesToRemoveOutlier.Add(new outlierModel
					{
						Date = day.Date,
						HistoricalValue = historicalDay.TotalStatisticCalculatedTasks,
						VariationValue = day.Tasks
					});
				}
			}

			var historicalDataWithoutOutliers = removeOutliers(valuesToRemoveOutlier);

			foreach (var taskOwner in historicalData.TaskOwnerDayCollection)
			{
				if(historicalDataWithoutOutliers.ContainsKey(taskOwner.CurrentDate))
					((ValidatedVolumeDay)taskOwner).ValidatedTasks = historicalDataWithoutOutliers[taskOwner.CurrentDate];
			}

			return historicalData;
		}

		private Dictionary<DateOnly, double> removeOutliers(List<outlierModel> valuesToRemoveOutlier)
		{
			var normalDistribution = new Dictionary<DateOnly, double>();
			foreach (var values in valuesToRemoveOutlier)
			{
				if (Math.Abs(values.HistoricalValue) > 0.001d)
				{
					normalDistribution.Add(values.Date, values.HistoricalValue - values.VariationValue);
				}
			}

			var descriptiveStatistics = new DescriptiveStatistics(normalDistribution.Values);
			var mean = descriptiveStatistics.Mean;
			var stdDev = descriptiveStatistics.StandardDeviation;
			var upper = mean + 3 * stdDev;
			var lower = mean - 3 * stdDev;

			var result = new Dictionary<DateOnly, double>();
			foreach (var day in normalDistribution)
			{
				if (day.Value > upper)
				{
					result[day.Key] = upper + valuesToRemoveOutlier.Single(x=>x.Date == day.Key).VariationValue;
				}
				else if(day.Value < lower)
				{
					result[day.Key] = lower + valuesToRemoveOutlier.Single(x => x.Date == day.Key).VariationValue;
				}
			}

			return result;
		}

		private class outlierModel
		{
			public DateOnly Date { get; set; }
			public double HistoricalValue { get; set; }
			public double VariationValue { get; set; }
		}
	}
}