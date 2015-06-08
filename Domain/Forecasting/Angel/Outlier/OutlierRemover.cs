using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Outlier
{
	public class OutlierRemover : IOutlierRemover
	{
		private const double outlierFactor = 0.5;

		public ITaskOwnerPeriod RemoveOutliers(ITaskOwnerPeriod historicalData, IForecastMethod forecastMethod)
		{
			var seasonalVariations = forecastMethod.SeasonalVariation(historicalData).ForecastingTargets;

			int historicalDayZeroExcludedCount = historicalData.TaskOwnerDayCollection.Count(x => Math.Abs(x.TotalStatisticCalculatedTasks) > 0.001d);
			var averageTasks = historicalData.TotalStatisticCalculatedTasks / historicalDayZeroExcludedCount;

			var upperThreshold = averageTasks * (1 + outlierFactor);
			var lowerThreshold = averageTasks * (1 - outlierFactor);

			var outlierDates = new Dictionary<DateOnly, double>();
			
			foreach (var seasonalDay in seasonalVariations)
			{
				foreach (var historicalDay in historicalData.TaskOwnerDayCollection)
				{
					if (seasonalDay.CurrentDate == historicalDay.CurrentDate)
					{
						var tasks = historicalDay.TotalStatisticCalculatedTasks - seasonalDay.Tasks  + averageTasks;
						if (tasks > upperThreshold)
							outlierDates.Add(seasonalDay.CurrentDate, upperThreshold);
						if (tasks < lowerThreshold)
							outlierDates.Add(seasonalDay.CurrentDate, lowerThreshold);
						break;
					}
				}
			}
			//const int q = 10;

			//foreach (var seasonalDay in seasonalVariations)
			//{
			//	double sum = 0;
			//	var average = 0;
			//	foreach (var historicalDay in historicalData.TaskOwnerDayCollection)
			//	{
			//		if (seasonalDay.CurrentDate < historicalDay.CurrentDate.AddDays(q))
			//		{
			//			//for (var i = 0; i < (q); i++)
			//			//{
			//			//	sum += historicalDay.TotalStatisticCalculatedTasks;
			//			//}
			//			//average = (int)(sum / (q));
			//		}
			//		else if (seasonalDay.CurrentDate.AddDays(q) > historicalDay.CurrentDate)
			//		{
			//			//for (var i = 0; i < (q); i++)
			//			//{
			//			//	sum += historicalDay.TotalStatisticCalculatedTasks;
			//			//}
			//			//average = (int)(sum / (q));
			//		}

			//		else if (seasonalDay.CurrentDate == historicalDay.CurrentDate.AddDays(q))
			//		{
			//			for (var i = 0; i < (2*q); i++)
			//			{
			//				sum += historicalDay.TotalStatisticCalculatedTasks;
			//			}
			//			average = (int) (sum/(2*q));
			//		}
			//		var x = (seasonalDay.Tasks - average)/average;
			//		if (x > outlierLimit)
			//		{
			//			outlierDates.Add(seasonalDay.CurrentDate);
			//		}
			//	}
			//}

			foreach (var outlierDate in outlierDates)
			{
				var taskOwner = historicalData.TaskOwnerDayCollection.Single(x => x.CurrentDate == outlierDate.Key);
				if (!(Math.Abs(taskOwner.TotalStatisticCalculatedTasks) < 0.001)) 
					((ValidatedVolumeDay)taskOwner).ValidatedTasks = outlierDate.Value;
				
			}

			return historicalData;
		}
	}
}