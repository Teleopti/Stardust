using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Outlier
{
	public class OutlierRemover : IOutlierRemover
	{
		private const double outlierLimit = 0.5;

		public TaskOwnerPeriod RemoveOutliers(TaskOwnerPeriod historicalData, IForecastMethod forecastMethod)
		{
			var forecastResult = forecastMethod.Forecast(historicalData, new DateOnlyPeriod(historicalData.StartDate, historicalData.EndDate));
			var seasonalVariations = forecastResult.ForecastingTargets;

			var averageTasks = historicalData.TotalStatisticCalculatedTasks / historicalData.TaskOwnerDayCollection.Count;

			var outlierDates = new List<DateOnly>();
			foreach (var seasonalDay in seasonalVariations)
			{
				foreach (var historicalDay in historicalData.TaskOwnerDayCollection)
				{
					if (seasonalDay.CurrentDate == historicalDay.CurrentDate)
					{
						var tasks = seasonalDay.Tasks - historicalDay.TotalStatisticCalculatedTasks + averageTasks;
						var upper = seasonalDay.Tasks * (1 + outlierLimit);
						var lower = seasonalDay.Tasks * (1 - outlierLimit);
						if (tasks > upper || tasks < lower)
						{
							outlierDates.Add(seasonalDay.CurrentDate);
						}
						break;
					}
				}
			}

			foreach (var outlierDate in outlierDates)
			{
				var taskOwner = historicalData.TaskOwnerDayCollection.Single(x => x.CurrentDate == outlierDate);
				if (!(Math.Abs(taskOwner.TotalStatisticCalculatedTasks) < 0.001)) 
					((ValidatedVolumeDay)taskOwner).ValidatedTasks = averageTasks;
				
			}

			return historicalData;
		}
	}
}