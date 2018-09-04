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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class QueueStatisticsViewModelFactory : IQueueStatisticsViewModelFactory
	{
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;
		private readonly IHistoricalData _historicalData;
		private readonly IOutlierRemover _outlierRemover;
		private readonly IForecastMethodProvider _forecastMethodProvider;
		private readonly IForecastWorkloadEvaluator _forecastWorkloadEvaluator;

		public QueueStatisticsViewModelFactory(
			IWorkloadRepository workloadRepository,
			IHistoricalPeriodProvider historicalPeriodProvider,
			IHistoricalData historicalData,
			IOutlierRemover outlierRemover,
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
			var workloadAccuracy = _forecastWorkloadEvaluator.Evaluate(workload, new TaskOutlierRemover(), new ForecastAccuracyCalculator());
			var forecastMethodId = (workloadAccuracy == null || workloadAccuracy.Accuracies.Length == 0)
				? ForecastMethodType.None
				: workloadAccuracy.Accuracies.Single(x => x.IsSelected).MethodId;
			return new WorkloadQueueStatisticsViewModel
			{
				WorkloadId = workload.Id.Value,
				QueueStatisticsDays = availablePeriod.HasValue
					? createQueueStatisticsDayViewModels(workload, forecastMethodId, availablePeriod.Value)
					: new List<QueueStatisticsModel>()
			};
		}

		private List<QueueStatisticsModel> createQueueStatisticsDayViewModels(IWorkload workload,
			ForecastMethodType method, DateOnlyPeriod period)
		{
			var historicalData = _historicalData.Fetch(workload, period);
			var forecastMethod = _forecastMethodProvider.Get(method);
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

			var historicalDataNoOutliers = _outlierRemover.RemoveOutliers(historicalData, forecastMethod);
			foreach (var day in historicalDataNoOutliers.TaskOwnerDayCollection)
			{
				statistics.Single(x => x.Date == day.CurrentDate).ValidatedTasks =
					Math.Round(day.TotalStatisticCalculatedTasks, 1);
			}

			return statistics;
		}
	}
}
