﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecasterWorkload : IQuickForecasterWorkload
	{
		private readonly IHistoricalData _historicalData;
		private readonly IFutureData _futureData;
		private readonly IForecastMethodProvider _forecastMethodProvider;
		private readonly IOutlierRemover _outlierRemover;

		public QuickForecasterWorkload(IHistoricalData historicalData, IFutureData futureData, IForecastMethodProvider forecastMethodProvider, IOutlierRemover outlierRemover)
		{
			_historicalData = historicalData;
			_futureData = futureData;
			_forecastMethodProvider = forecastMethodProvider;
			_outlierRemover = outlierRemover;
		}

		public ForecastModel Execute(QuickForecasterWorkloadParams quickForecasterWorkloadParams)
		{
			var historicalData = _historicalData.Fetch(quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.HistoricalPeriod);
			if (!historicalData.TaskOwnerDayCollection.Any())
			{
				return new ForecastModel
				{
					WorkloadId = quickForecasterWorkloadParams.WorkLoad.Id.Value,
					ScenarioId = quickForecasterWorkloadParams.Scenario.Id.Value,
					ForecastDays = new List<ForecastDayModel>()
				};
			}

			var forecastMethod = _forecastMethodProvider.Get(quickForecasterWorkloadParams.ForecastMethodId);
			var historicalDataNoOutliers = _outlierRemover.RemoveOutliers(historicalData, forecastMethod);
			var forecast =  forecastMethod.Forecast(historicalDataNoOutliers, quickForecasterWorkloadParams.FuturePeriod);
			var workloadDays = _futureData.Fetch(quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.SkillDays, quickForecasterWorkloadParams.FuturePeriod);
			var forecastDays = createForecastViewModel(forecast, workloadDays);
			return new ForecastModel
			{
				WorkloadId = quickForecasterWorkloadParams.WorkLoad.Id.Value,
				ScenarioId = quickForecasterWorkloadParams.Scenario.Id.Value,
				ForecastDays = forecastDays
			};
		}

		private static List<ForecastDayModel> createForecastViewModel(ForecastMethodResult forecast, IEnumerable<IWorkloadDayBase> workloadDays)
		{
			var forecastResult = new List<ForecastDayModel>();
			if (forecast.ForecastingTargets == null)
				return forecastResult;
			var workloadDayForDate = workloadDays
				.ToDictionary(w => w.CurrentDate);
			
			foreach (var target in forecast.ForecastingTargets)
			{
				var currentWorkLoadDay = workloadDayForDate[target.CurrentDate];
				var model = new ForecastDayModel
				{
					Date = target.CurrentDate,
					Tasks = target.Tasks,
					AverageTaskTime = target.AverageTaskTime.TotalSeconds,
					AverageAfterTaskTime = target.AverageAfterTaskTime.TotalSeconds,
					TotalTasks = target.Tasks,
					TotalAverageTaskTime = target.AverageTaskTime.TotalSeconds,
					TotalAverageAfterTaskTime = target.AverageAfterTaskTime.TotalSeconds,
					IsOpen = currentWorkLoadDay.OpenForWork.IsOpen
				};

				if (hasCampaign(currentWorkLoadDay) && hasOverride(currentWorkLoadDay)) model.CampaignAndOverride = -1;
				if (hasCampaign(currentWorkLoadDay))
				{
					model.Campaign = -1;
					model.TotalTasks = model.Tasks * (currentWorkLoadDay.CampaignTasks.Value + 1d);
					model.TotalAverageTaskTime = model.TotalAverageTaskTime * (currentWorkLoadDay.CampaignTaskTime.Value + 1d);
					model.TotalAverageAfterTaskTime = model.TotalAverageAfterTaskTime * (currentWorkLoadDay.CampaignAfterTaskTime.Value + 1d);
				}
				if (hasOverride(currentWorkLoadDay))
				{
					model.Override = -1;
					model.TotalTasks = currentWorkLoadDay.OverrideTasks ?? model.Tasks;
					model.TotalAverageTaskTime = currentWorkLoadDay.OverrideAverageTaskTime?.TotalSeconds ?? model.TotalAverageTaskTime;
					model.TotalAverageAfterTaskTime = currentWorkLoadDay.OverrideAverageAfterTaskTime?.TotalSeconds ?? model.TotalAverageAfterTaskTime;
				}

				forecastResult.Add(model);
			}

			return forecastResult;
		}

		private static bool hasCampaign(IWorkloadDayBase workloadDay)
		{
			return workloadDay.CampaignTasks.Value > 0d || 
				   workloadDay.CampaignTaskTime.Value > 0d ||
				   workloadDay.CampaignAfterTaskTime.Value > 0d;
		}

		private static bool hasOverride(IWorkloadDayBase workloadDay)
		{
			return workloadDay.OverrideTasks.HasValue ||
				   workloadDay.OverrideAverageTaskTime.HasValue ||
				   workloadDay.OverrideAverageAfterTaskTime.HasValue;
		}
	}
}