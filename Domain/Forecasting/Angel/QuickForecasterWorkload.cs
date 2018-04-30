using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

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

		public IList<ForecastResultModel> Execute(QuickForecasterWorkloadParams quickForecasterWorkloadParams)
		{
			var historicalData = _historicalData.Fetch(quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.HistoricalPeriod);
			if (!historicalData.TaskOwnerDayCollection.Any())
				return new List<ForecastResultModel>();
			var forecastMethod = _forecastMethodProvider.Get(quickForecasterWorkloadParams.ForecastMethodId);
			var historicalDataNoOutliers = _outlierRemover.RemoveOutliers(historicalData, forecastMethod);
			var forecast =  forecastMethod.Forecast(historicalDataNoOutliers, quickForecasterWorkloadParams.FuturePeriod);
			var workloadDays = _futureData.Fetch(quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.SkillDays, quickForecasterWorkloadParams.FuturePeriod);
			return forecast.ForecastingTargets == null ? new List<ForecastResultModel>() : createForecastViewModel(forecast, workloadDays);
		}

		private static IList<ForecastResultModel> createForecastViewModel(ForecastMethodResult forecast, IEnumerable<IWorkloadDayBase> workloadDays)
		{
			var workloadDayForDate = workloadDays
				.ToDictionary(w => w.CurrentDate);
			var taskOwnerHelper = new TaskOwnerHelper(workloadDays);
			taskOwnerHelper.BeginUpdate();
			var forecastresult = new List<ForecastResultModel>();
			foreach (var target in forecast.ForecastingTargets)
			{
				var model = new ForecastResultModel
				{
					date = target.CurrentDate.Date,
					vc = target.Tasks,
					vtc = target.Tasks,
					vtt = target.AverageTaskTime.TotalSeconds,
					vttt = target.AverageTaskTime.TotalSeconds,
					vacw = target.AverageAfterTaskTime.TotalSeconds,
					vtacw = target.AverageAfterTaskTime.TotalSeconds
				};
				var currentWorkLoadDay = workloadDayForDate[target.CurrentDate];
				if (hasCampaign(currentWorkLoadDay) && hasOverride(currentWorkLoadDay))
				{
					model.vcombo = -1;
				}
				if(hasCampaign(currentWorkLoadDay))
				{
					model.vcampaign = -1;
					model.vtc = model.vc * (currentWorkLoadDay.CampaignTasks.Value + 1d);
					model.vtt = model.vtt * (currentWorkLoadDay.CampaignTaskTime.Value + 1d);
					model.vtacw = model.vacw * (currentWorkLoadDay.CampaignAfterTaskTime.Value + 1d);
				}
				if (hasOverride(currentWorkLoadDay))
				{
					model.voverride = -1;
					model.vtc = currentWorkLoadDay.OverrideTasks ?? model.vc;
					model.vtt = currentWorkLoadDay.OverrideAverageTaskTime?.TotalSeconds ?? model.vtt;
					model.vtacw = currentWorkLoadDay.OverrideAverageAfterTaskTime?.TotalSeconds ?? model.vacw;
				}

				forecastresult.Add(model);
			}

			return forecastresult;
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