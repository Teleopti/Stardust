using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecasterWorkload : IQuickForecasterWorkload
	{
		private readonly IHistoricalData _historicalData;
		private readonly IFutureData _futureData;
		private readonly IForecastMethodProvider _forecastMethodProvider;
		private readonly IOutlierRemover _outlierRemover;
		private readonly ForecastDayModelMapper _forecastDayModelMapper;
		private readonly IForecastDayOverrideRepository _forecastDayOverrideRepository;

		public QuickForecasterWorkload(IHistoricalData historicalData, IFutureData futureData,
			IForecastMethodProvider forecastMethodProvider, IOutlierRemover outlierRemover,
			ForecastDayModelMapper forecastDayModelMapper, IForecastDayOverrideRepository forecastDayOverrideRepository)
		{
			_historicalData = historicalData;
			_futureData = futureData;
			_forecastMethodProvider = forecastMethodProvider;
			_outlierRemover = outlierRemover;
			_forecastDayModelMapper = forecastDayModelMapper;
			_forecastDayOverrideRepository = forecastDayOverrideRepository;
		}

		public ForecastModel Execute(QuickForecasterWorkloadParams quickForecasterWorkloadParams)
		{
			var historicalData = _historicalData.Fetch(quickForecasterWorkloadParams.WorkLoad,
				quickForecasterWorkloadParams.HistoricalPeriod);
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
			if (forecastMethod == null)
			{
				return new ForecastModel
				{
					WorkloadId = quickForecasterWorkloadParams.WorkLoad.Id.Value,
					ScenarioId = quickForecasterWorkloadParams.Scenario.Id.Value,
					ForecastDays = new List<ForecastDayModel>(),
					WarningMessage = Resources.NoQueueStatisticsAvailable
				};
			}
			var historicalDataNoOutliers = _outlierRemover.RemoveOutliers(historicalData, forecastMethod);
			var forecastResult = forecastMethod.ForecastTasks(historicalDataNoOutliers, quickForecasterWorkloadParams.FuturePeriod);

			var workloadDays = _futureData.Fetch(quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.SkillDays,
				quickForecasterWorkloadParams.FuturePeriod);
			var overrideDays = _forecastDayOverrideRepository.FindRange(quickForecasterWorkloadParams.FuturePeriod,
				quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.Scenario).ToDictionary(k => k.Date);
			var forecastDays = createForecastViewModel(forecastResult, workloadDays, overrideDays);
			return new ForecastModel
			{
				WorkloadId = quickForecasterWorkloadParams.WorkLoad.Id.Value,
				ScenarioId = quickForecasterWorkloadParams.Scenario.Id.Value,
				ForecastDays = forecastDays
			};
		}

		private List<ForecastDayModel> createForecastViewModel(IDictionary<DateOnly, double> forecastTargets,
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
					Tasks = target.Value,
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