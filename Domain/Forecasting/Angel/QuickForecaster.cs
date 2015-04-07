using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecaster : IQuickForecaster
	{
		private readonly IQuickForecasterWorkload _quickForecasterWorkload;
		private readonly IFetchAndFillSkillDays _fetchAndFillSkillDays;
		private readonly IQuickForecastWorkloadEvaluator _quickForecastWorkloadEvaluator;

		public QuickForecaster(IQuickForecasterWorkload quickForecasterWorkload, IFetchAndFillSkillDays fetchAndFillSkillDays, IQuickForecastWorkloadEvaluator quickForecastWorkloadEvaluator)
		{
			_quickForecasterWorkload = quickForecasterWorkload;
			_fetchAndFillSkillDays = fetchAndFillSkillDays;
			_quickForecastWorkloadEvaluator = quickForecastWorkloadEvaluator;
		}

		public void ForecastWorkloadsWithinSkill(ISkill skill, ForecastWorkloadInput[] workloads, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriodForForecast, DateOnlyPeriod historicalPeriodForMeasurement)
		{
			var skillDays = _fetchAndFillSkillDays.FindRange(futurePeriod, skill);

			foreach (var workload in skill.WorkloadCollection)
			{
				var workloadInput = workloads.SingleOrDefault(x => x.WorkloadId == workload.Id.Value);
				if (workloadInput!=null)
				{
					var workloadAccuracy = _quickForecastWorkloadEvaluator.Measure(workload, historicalPeriodForMeasurement);
					var quickForecasterWorkloadParams = new QuickForecasterWorkloadParams
					{
						WorkLoad = workload,
						FuturePeriod = futurePeriod,
						SkillDays = skillDays,
						HistoricalPeriod = historicalPeriodForForecast,
						ForecastMethodId = workloadAccuracy.Accuracies.Length == 0 ? ForecastMethodType.TeleoptiClassic : workloadAccuracy.Accuracies.Single(x => x.IsSelected).MethodId
					};
					_quickForecasterWorkload.Execute(quickForecasterWorkloadParams);
				}
			}
		}

		public void ForecastAll(ISkill skill, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod)
		{
			var skillDays = _fetchAndFillSkillDays.FindRange(futurePeriod, skill);

			foreach (var workload in skill.WorkloadCollection)
			{
				var quickForecasterWorkloadParams = new QuickForecasterWorkloadParams
				{
					WorkLoad = workload,
					FuturePeriod = futurePeriod,
					SkillDays = skillDays,
					HistoricalPeriod = historicalPeriod
				};
				_quickForecasterWorkload.Execute(quickForecasterWorkloadParams);
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