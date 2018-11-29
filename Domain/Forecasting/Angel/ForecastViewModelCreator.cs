using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastViewModelCreator
	{
		private readonly ForecasterWorkload _quickForecasterWorkload;
		private readonly IFetchAndFillSkillDays _fetchAndFillSkillDays;
		private readonly IForecastWorkloadEvaluator _forecastWorkloadEvaluator;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;

		public ForecastViewModelCreator(ForecasterWorkload quickForecasterWorkload, IFetchAndFillSkillDays fetchAndFillSkillDays,
			IForecastWorkloadEvaluator forecastWorkloadEvaluator, IHistoricalPeriodProvider historicalPeriodProvider)
		{
			_quickForecasterWorkload = quickForecasterWorkload;
			_fetchAndFillSkillDays = fetchAndFillSkillDays;
			_forecastWorkloadEvaluator = forecastWorkloadEvaluator;
			_historicalPeriodProvider = historicalPeriodProvider;
		}

		public ForecastViewModel ForecastWorkload(IWorkload workload, DateOnlyPeriod futurePeriod, IScenario scenario)
		{
			var availablePeriod = _historicalPeriodProvider.AvailablePeriod(workload);
			var result = new ForecastViewModel
			{
				WorkloadId = workload.Id.GetValueOrDefault(),
				ScenarioId = scenario.Id.Value,
				ForecastDays = new List<ForecastDayModel>(),
				WarningMessage = Resources.NoQueueStatisticsAvailable
			};

			if (!availablePeriod.HasValue)
			{
				return result;
			}

			var availableIntradayTemplatePeriod = _historicalPeriodProvider.AvailableIntradayTemplatePeriod(availablePeriod.Value);
			var skillDays = _fetchAndFillSkillDays.FindRange(futurePeriod, workload.Skill, scenario);
			var bestForecastMethods = _forecastWorkloadEvaluator.Evaluate(workload, new OutlierRemover(), new ForecastAccuracyCalculator());
			if (bestForecastMethods.ForecastMethodTypeForTasks == ForecastMethodType.None)
			{
				return result;
			}
			var quickForecasterWorkloadParams = new QuickForecasterWorkloadParams
			{
				Scenario = scenario,
				WorkLoad = workload,
				FuturePeriod = futurePeriod,
				SkillDays = skillDays,
				HistoricalPeriod = availablePeriod.Value,
				IntradayTemplatePeriod = availableIntradayTemplatePeriod,
				BestForecastMethods = bestForecastMethods
			};
			result.ForecastDays = _quickForecasterWorkload.Execute(quickForecasterWorkloadParams);
			result.WarningMessage = null;
			return result;
		}
	}

	public struct QuickForecasterWorkloadParams
	{
		public IWorkload WorkLoad { get; set; }
		public DateOnlyPeriod FuturePeriod { get; set; }
		public ICollection<ISkillDay> SkillDays { get; set; }
		public DateOnlyPeriod HistoricalPeriod { get; set; }
		public DateOnlyPeriod IntradayTemplatePeriod { get; set; }
		public WorkloadAccuracy BestForecastMethods { get; set; }
		public IScenario Scenario { get; set; }
	}
}