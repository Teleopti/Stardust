using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecasterWorkload 
	{
		private readonly IHistoricalData _historicalData;
		private readonly IFutureData _futureData;
		private readonly IForecastMethodProvider _forecastMethodProvider;
		private readonly OutlierRemover _outlierRemover;
		private readonly ForecastDayModelMapper _forecastDayModelMapper;
		private readonly IForecastDayOverrideRepository _forecastDayOverrideRepository;

		public ForecasterWorkload(IHistoricalData historicalData, IFutureData futureData,
			IForecastMethodProvider forecastMethodProvider, OutlierRemover outlierRemover,
			ForecastDayModelMapper forecastDayModelMapper, IForecastDayOverrideRepository forecastDayOverrideRepository)
		{
			_historicalData = historicalData;
			_futureData = futureData;
			_forecastMethodProvider = forecastMethodProvider;
			_outlierRemover = outlierRemover;
			_forecastDayModelMapper = forecastDayModelMapper;
			_forecastDayOverrideRepository = forecastDayOverrideRepository;
		}

		public IList<ForecastDayModel> Execute(QuickForecasterWorkloadParams quickForecasterWorkloadParams)
		{
			var historicalData = _historicalData.Fetch(quickForecasterWorkloadParams.WorkLoad,
				quickForecasterWorkloadParams.HistoricalPeriod);
			if (!historicalData.TaskOwnerDayCollection.Any())
			{
				return new List<ForecastDayModel>();
			}

			var forecastMethodForTasks = _forecastMethodProvider.Get(quickForecasterWorkloadParams.BestForecastMethods.ForecastMethodTypeForTasks);
			var forecastMethodForTaskTime = _forecastMethodProvider.Get(quickForecasterWorkloadParams.BestForecastMethods.ForecastMethodTypeForTaskTime);
			var forecastMethodForAfterTaskTime = _forecastMethodProvider.Get(quickForecasterWorkloadParams.BestForecastMethods.ForecastMethodTypeForAfterTaskTime);
			if (forecastMethodForTasks == null)
			{
				return new List<ForecastDayModel>();
			}
			var queueStatisticsWithoutOutliers = _outlierRemover.RemoveOutliers(historicalData, forecastMethodForTasks, forecastMethodForTaskTime, forecastMethodForAfterTaskTime);

			foreach (var taskOwner in historicalData.TaskOwnerDayCollection)
			{
				if (queueStatisticsWithoutOutliers.Tasks.ContainsKey(taskOwner.CurrentDate))
					((ValidatedVolumeDay)taskOwner).ValidatedTasks =
					queueStatisticsWithoutOutliers.Tasks[taskOwner.CurrentDate];

				if (queueStatisticsWithoutOutliers.TaskTime.ContainsKey(taskOwner.CurrentDate))
					((ValidatedVolumeDay)taskOwner).ValidatedAverageTaskTime =
					TimeSpan.FromSeconds(queueStatisticsWithoutOutliers.TaskTime[taskOwner.CurrentDate]);

				if (queueStatisticsWithoutOutliers.AfterTaskTime.ContainsKey(taskOwner.CurrentDate))
					((ValidatedVolumeDay)taskOwner).ValidatedAverageAfterTaskTime =
					TimeSpan.FromSeconds(queueStatisticsWithoutOutliers.AfterTaskTime[taskOwner.CurrentDate]);
			}

			var forecastedTasks = forecastMethodForTasks.ForecastTasks(historicalData, quickForecasterWorkloadParams.FuturePeriod);
			var forecastedTaskTime = forecastMethodForTaskTime.ForecastTaskTime(historicalData, quickForecasterWorkloadParams.FuturePeriod);
			var forecastedAfterTaskTime = forecastMethodForAfterTaskTime.ForecastAfterTaskTime(historicalData, quickForecasterWorkloadParams.FuturePeriod);

			var workloadDays = _futureData.Fetch(quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.SkillDays,
				quickForecasterWorkloadParams.FuturePeriod);
			var overrideDays = _forecastDayOverrideRepository.FindRange(quickForecasterWorkloadParams.FuturePeriod,
				quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.Scenario).ToDictionary(k => k.Date);
 			return createForecastViewModel(forecastedTasks, forecastedTaskTime, forecastedAfterTaskTime, workloadDays, overrideDays);
 		}

		private IList<ForecastDayModel> createForecastViewModel(IDictionary<DateOnly, double> forecastTargets,
			IDictionary<DateOnly, TimeSpan> forecastedTaskTime, IDictionary<DateOnly, TimeSpan> forecastedAfterTaskTime,
			IEnumerable<IWorkloadDayBase> workloadDays, Dictionary<DateOnly, IForecastDayOverride> overrideDays)
		{
			var forecastResult = new List<ForecastDayModel>();
			if (forecastTargets == null)
				return forecastResult;
			var workloadDayForDate = workloadDays
				.ToDictionary(w => w.CurrentDate);

			foreach (var target in forecastTargets)
			{
				overrideDays.TryGetValue(target.Key, out var overrideDay);
				var currentWorkLoadDay = workloadDayForDate[target.Key];
				var model = new ForecastDayModel
				{
					Date = target.Key,
					IsForecasted = true,
					Tasks = target.Value,
					TotalTasks = target.Value,
					AverageTaskTime = forecastedTaskTime[target.Key].TotalSeconds,
					TotalAverageTaskTime = forecastedTaskTime[target.Key].TotalSeconds,
					AverageAfterTaskTime = forecastedAfterTaskTime[target.Key].TotalSeconds,
					TotalAverageAfterTaskTime = forecastedAfterTaskTime[target.Key].TotalSeconds,
					IsOpen = currentWorkLoadDay.OpenForWork.IsOpen,
					IsInModification = currentWorkLoadDay.OpenForWork.IsOpen
				};

				_forecastDayModelMapper.SetCampaignAndOverride(currentWorkLoadDay, model, overrideDay);
				forecastResult.Add(model);
			}

			return forecastResult;
		}
	}
}