using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
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

		public void ForecastWorkloadsWithinSkill(ISkill skill, ForecastWorkloadInput[] workloads, DateOnlyPeriod futurePeriod)
		{
			var skillDays = _fetchAndFillSkillDays.FindRange(futurePeriod, skill);

			foreach (var workload in skill.WorkloadCollection)
			{
				var workloadInput = workloads.SingleOrDefault(x => x.WorkloadId == workload.Id.Value);
				if (workloadInput != null)
				{
					var forecastMethodId = workloadInput.ForecastMethodId;
					if (forecastMethodId == ForecastMethodType.None)
					{
						var workloadAccuracy = _forecastWorkloadEvaluator.Evaluate(workload);
						forecastMethodId = (workloadAccuracy == null || workloadAccuracy.Accuracies.Length == 0)
							? ForecastMethodType.TeleoptiClassic
							: workloadAccuracy.Accuracies.Single(x => x.IsSelected).MethodId;
					}
					
					var quickForecasterWorkloadParams = new QuickForecasterWorkloadParams
					{
						WorkLoad = workload,
						FuturePeriod = futurePeriod,
						SkillDays = skillDays,
						HistoricalPeriod = _historicalPeriodProvider.PeriodForForecast(workload),
						ForecastMethodId = forecastMethodId
					};
					_quickForecasterWorkload.Execute(quickForecasterWorkloadParams);
				}
			}
		}
	}

	public struct QuickForecasterWorkloadParams
	{
		public IWorkload WorkLoad { get; set; }
		public DateOnlyPeriod FuturePeriod { get; set; }
		public ICollection<ISkillDay> SkillDays { get; set; }
		public DateOnlyPeriod HistoricalPeriod { get; set; }
		public ForecastMethodType ForecastMethodId { get; set; }
	}
}