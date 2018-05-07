using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecaster : IQuickForecaster
	{
		private readonly IQuickForecasterWorkload _quickForecasterWorkload;
		private readonly IFetchAndFillSkillDays _fetchAndFillSkillDays;
		private readonly IForecastWorkloadEvaluator _forecastWorkloadEvaluator;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;

		public QuickForecaster(IQuickForecasterWorkload quickForecasterWorkload, IFetchAndFillSkillDays fetchAndFillSkillDays, IForecastWorkloadEvaluator forecastWorkloadEvaluator, IHistoricalPeriodProvider historicalPeriodProvider)
		{
			_quickForecasterWorkload = quickForecasterWorkload;
			_fetchAndFillSkillDays = fetchAndFillSkillDays;
			_forecastWorkloadEvaluator = forecastWorkloadEvaluator;
			_historicalPeriodProvider = historicalPeriodProvider;
		}

		public ForecastModel ForecastWorkloadWithinSkill(ISkill skill, ForecastWorkloadInput workloadInput, DateOnlyPeriod futurePeriod, IScenario scenario)
		{
			var skillDays = _fetchAndFillSkillDays.FindRange(futurePeriod, skill, scenario);
			var workload = skill.WorkloadCollection.Single(w => w.Id.Value == workloadInput.WorkloadId);

			var forecastMethodId = workloadInput.ForecastMethodId;
			if (forecastMethodId == ForecastMethodType.None)
			{
				var workloadAccuracy = _forecastWorkloadEvaluator.Evaluate(workload);
				forecastMethodId = (workloadAccuracy == null || workloadAccuracy.Accuracies.Length == 0)
					? ForecastMethodType.None
					: workloadAccuracy.Accuracies.Single(x => x.IsSelected).MethodId;
			}

			var availablePeriod = _historicalPeriodProvider.AvailablePeriod(workload);
			if (!availablePeriod.HasValue)
			{
				return new ForecastModel
				{
					WorkloadId = workloadInput.WorkloadId,
					ScenarioId = scenario.Id.Value,
					ForecastDays = new List<ForecastDayModel>()
				};
			}

			var availableIntradayTemplatePeriod = _historicalPeriodProvider.AvailableIntradayTemplatePeriod(availablePeriod.Value);
			var quickForecasterWorkloadParams = new QuickForecasterWorkloadParams
			{
				Scenario = scenario,
				WorkLoad = workload,
				FuturePeriod = futurePeriod,
				SkillDays = skillDays,
				HistoricalPeriod = availablePeriod.Value,
				IntradayTemplatePeriod = availableIntradayTemplatePeriod,
				ForecastMethodId = forecastMethodId
			};
			return _quickForecasterWorkload.Execute(quickForecasterWorkloadParams);
		}
	}

	public struct QuickForecasterWorkloadParams
	{
		public IWorkload WorkLoad { get; set; }
		public DateOnlyPeriod FuturePeriod { get; set; }
		public ICollection<ISkillDay> SkillDays { get; set; }
		public DateOnlyPeriod HistoricalPeriod { get; set; }
		public DateOnlyPeriod IntradayTemplatePeriod { get; set; }
		public ForecastMethodType ForecastMethodId { get; set; }
		public IScenario Scenario { get; set; }
	}
}