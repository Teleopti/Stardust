using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class QueueStatisticsViewModelFactory : IQueueStatisticsViewModelFactory
	{
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;
		private readonly IHistoricalData _historicalData;
		private readonly OutlierRemover _outlierRemover;
		private readonly IForecastMethodProvider _forecastMethodProvider;
		private readonly IForecastWorkloadEvaluator _forecastWorkloadEvaluator;

		public QueueStatisticsViewModelFactory(
			IWorkloadRepository workloadRepository,
			IHistoricalPeriodProvider historicalPeriodProvider,
			IHistoricalData historicalData,
			OutlierRemover outlierRemover,
			IForecastMethodProvider forecastMethodProvider,
			IForecastWorkloadEvaluator forecastWorkloadEvaluator)
		{
			_workloadRepository = workloadRepository;
			_historicalPeriodProvider = historicalPeriodProvider;
			_historicalData = historicalData;
			_outlierRemover = outlierRemover;
			_forecastMethodProvider = forecastMethodProvider;
			_forecastWorkloadEvaluator = forecastWorkloadEvaluator;
		}

		public WorkloadQueueStatisticsViewModel QueueStatistics(Guid workloadId)
		{
			var workload = _workloadRepository.Get(workloadId);
			var availablePeriod = _historicalPeriodProvider.AvailablePeriod(workload);
			var workloadAccuracy = _forecastWorkloadEvaluator.Evaluate(workload, new OutlierRemover(), new ForecastAccuracyCalculator());
			var forecastMethodIdForTasks = workloadAccuracy.ForecastMethodTypeForTasks;
			var forecastMethodIdForTaskTime = workloadAccuracy.ForecastMethodTypeForTaskTime;
			var forecastMethodIdForAfterTaskTime = workloadAccuracy.ForecastMethodTypeForAfterTaskTime;
			return new WorkloadQueueStatisticsViewModel
			{
				WorkloadId = workload.Id.Value,
				QueueStatisticsDays = availablePeriod.HasValue
					? createQueueStatisticsDayViewModels(workload, forecastMethodIdForTasks, forecastMethodIdForTaskTime,
						forecastMethodIdForAfterTaskTime, availablePeriod.Value)
					: new List<QueueStatisticsModel>()
			};
		}

		private List<QueueStatisticsModel> createQueueStatisticsDayViewModels(IWorkload workload,
			ForecastMethodType methodForTasks, ForecastMethodType methodForTaskTime,
			ForecastMethodType methodForAfterTaskTime, DateOnlyPeriod period)
		{
			var historicalData = _historicalData.Fetch(workload, period);
			var forecastMethodForTasks = _forecastMethodProvider.Get(methodForTasks);
			var forecastMethodForTaskTime = _forecastMethodProvider.Get(methodForTaskTime);
			var forecastMethodForAfterTaskTime = _forecastMethodProvider.Get(methodForAfterTaskTime);
			var statistics = new List<QueueStatisticsModel>();
			foreach (var taskOwner in historicalData.TaskOwnerDayCollection)
			{
				var dayStats = new QueueStatisticsModel
				{
					Date = taskOwner.CurrentDate,
					OriginalTasks = taskOwner.TotalStatisticCalculatedTasks
				};
				statistics.Add(dayStats);
			}

			var historicalDataNoOutliers = _outlierRemover.RemoveOutliers(historicalData, forecastMethodForTasks,
				forecastMethodForTaskTime, forecastMethodForAfterTaskTime);
			foreach (var day in historicalDataNoOutliers.Tasks)
			{
				statistics.Single(x => x.Date == day.Key).ValidatedTasks =
					Math.Round(day.Value, 1);
			}

			return statistics;
		}
	}
}
