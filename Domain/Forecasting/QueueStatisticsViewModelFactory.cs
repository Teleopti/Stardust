using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel;
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

		public QueueStatisticsViewModelFactory(
			IWorkloadRepository workloadRepository, 
			IHistoricalPeriodProvider historicalPeriodProvider,
			IHistoricalData historicalData,
			IOutlierRemover outlierRemover,
			IForecastMethodProvider forecastMethodProvider)
		{
			_workloadRepository = workloadRepository;
			_historicalPeriodProvider = historicalPeriodProvider;
			_historicalData = historicalData;
			_outlierRemover = outlierRemover;
			_forecastMethodProvider = forecastMethodProvider;
		}
		public WorkloadQueueStatisticsViewModel QueueStatistics(QueueStatisticsInput input)
		{
			var workload = _workloadRepository.Get(input.WorkloadId);
			var availablePeriod = _historicalPeriodProvider.AvailablePeriod(workload);
			return new WorkloadQueueStatisticsViewModel
			{
				WorkloadId = workload.Id.Value,
				QueueStatisticsDays = availablePeriod.HasValue
					? CreateQueueStatisticsDayViewModels(workload, input.MethodId, availablePeriod.Value)
					: new List<QueueStatisticsModel>()
			};
		}

		public List<QueueStatisticsModel> CreateQueueStatisticsDayViewModels(IWorkload workload, ForecastMethodType method, DateOnlyPeriod period)
		{
			var historicalData = _historicalData.Fetch(workload, period);
			var forecastMethod = _forecastMethodProvider.Get(method);
			var statistics = new List<QueueStatisticsModel>();
			foreach (var taskOwner in historicalData.TaskOwnerDayCollection)
			{
				var dayStats = new QueueStatisticsModel();
				dayStats.Date = taskOwner.CurrentDate;
				dayStats.Tasks = taskOwner.TotalStatisticCalculatedTasks;
				statistics.Add(dayStats);
			}

			var historicalDataNoOutliers = _outlierRemover.RemoveOutliers(historicalData, forecastMethod);
			foreach (var day in historicalDataNoOutliers.TaskOwnerDayCollection)
			{
				statistics.Single(x => x.Date == day.CurrentDate).OutlierTasks = Math.Round(day.TotalStatisticCalculatedTasks, 1);
			}
			return statistics;
		}
	}
}
