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
			var seasonalVariations = forecastMethod.SeasonalVariation(historicalData).ForecastingTargets;

			var tasksWithoutSeasonalVariations = new Dictionary<DateOnly, double>();
			
			foreach (var seasonalDay in seasonalVariations)
			{
				foreach (var historicalDay in historicalData.TaskOwnerDayCollection)
				{
					if (seasonalDay.CurrentDate == historicalDay.CurrentDate)
					{
						if (Math.Abs(historicalDay.TotalStatisticCalculatedTasks) > 0.001d)
							tasksWithoutSeasonalVariations.Add(seasonalDay.CurrentDate, historicalDay.TotalStatisticCalculatedTasks - seasonalDay.Tasks);
						break;
					}
				}
			}

			var descriptiveStatistics=new DescriptiveStatistics(tasksWithoutSeasonalVariations.Values);
			var mean = descriptiveStatistics.Mean;
			var stdDev = descriptiveStatistics.StandardDeviation;
			var upper = mean + 3*stdDev;
			var lower = mean - 3*stdDev;

			foreach (var day in tasksWithoutSeasonalVariations)
			{
				if (day.Value > upper)
				{
					var taskOwner = historicalData.TaskOwnerDayCollection.Single(x => x.CurrentDate == day.Key);
					((ValidatedVolumeDay)taskOwner).ValidatedTasks = upper;
				}
				else if (day.Value < lower)
				{
					var taskOwner = historicalData.TaskOwnerDayCollection.Single(x => x.CurrentDate == day.Key);
					((ValidatedVolumeDay)taskOwner).ValidatedTasks = lower;
				}
			}

			return historicalData;
		}
	}
}