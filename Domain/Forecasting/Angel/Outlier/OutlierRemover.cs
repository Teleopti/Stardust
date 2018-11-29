using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Outlier
{
	public class OutlierRemover
	{
		public QueueStatisticsPerType RemoveOutliers(ITaskOwnerPeriod historicalData, IForecastMethod forecastMethodForTasks,
			IForecastMethod forecastMethodForTaskTime, IForecastMethod forecastMethodForAfterTaskTime)
		{
			var seasonalVariationForTasks =
				forecastMethodForTasks.SeasonalVariationTasks(historicalData);
			var seasonalVariationForTaskTime =
				forecastMethodForTaskTime.SeasonalVariationTaskTime(historicalData);
			var seasonalVariationForAfterTaskTime =
				forecastMethodForAfterTaskTime.SeasonalVariationAfterTaskTime(historicalData);

			var historicalDictionary = historicalData.TaskOwnerDayCollection.ToDictionary(k => k.CurrentDate);

			var tasksOutlierModel = new List<outlierModel>();
			var taskTimeOutlierModel = new List<outlierModel>();
			var afterTaskTimeOutlierModel = new List<outlierModel>();

			foreach (var day in seasonalVariationForTasks)
			{
				if(!historicalDictionary.TryGetValue(day.Key, out var owner)) continue;
				tasksOutlierModel.Add(new outlierModel
					{
						Date = day.Key,
						HistoricalValue = owner.TotalStatisticCalculatedTasks,
						VariationValue = day.Value
					});
			}

			foreach (var day in seasonalVariationForTaskTime)
			{
				if (!historicalDictionary.TryGetValue(day.Key, out var owner)) continue;
				taskTimeOutlierModel.Add(new outlierModel
					{
						Date = day.Key,
						HistoricalValue = owner.TotalStatisticAverageTaskTime.TotalSeconds,
						VariationValue = day.Value
					});
			}

			foreach (var day in seasonalVariationForAfterTaskTime)
			{
				if (!historicalDictionary.TryGetValue(day.Key, out var owner)) continue;
				afterTaskTimeOutlierModel.Add(new outlierModel
					{
						Date = day.Key,
						HistoricalValue = owner.TotalStatisticAverageAfterTaskTime.TotalSeconds,
						VariationValue = day.Value
					});
			}

			return new QueueStatisticsPerType
			{
				Tasks = removeOutliers(tasksOutlierModel),
				TaskTime = removeOutliers(taskTimeOutlierModel),
				AfterTaskTime = removeOutliers(afterTaskTimeOutlierModel)
			};
		}

		private Dictionary<DateOnly, double> removeOutliers(IReadOnlyCollection<outlierModel> valuesToRemoveOutlier)
		{
			var normalDistribution = valuesToRemoveOutlier
				.Where(values => Math.Abs(values.HistoricalValue) > 0.001d)
				.ToDictionary(values => values.Date, values => values.HistoricalValue - values.VariationValue);

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
					result[day.Key] = upper + valuesToRemoveOutlier.Single(x => x.Date == day.Key).VariationValue;
				}
				else if (day.Value < lower)
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