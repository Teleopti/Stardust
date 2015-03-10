using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecaster : IQuickForecaster
	{
		private readonly IQuickForecasterWorkload _quickForecasterWorkload;
		private readonly IFetchAndFillSkillDays _fetchAndFillSkillDays;

		public QuickForecaster(IQuickForecasterWorkload quickForecasterWorkload, IFetchAndFillSkillDays fetchAndFillSkillDays)
		{
			_quickForecasterWorkload = quickForecasterWorkload;
			_fetchAndFillSkillDays = fetchAndFillSkillDays;
		}

		public double ForecastForSkill(ISkill skill, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod)
		{
			var skillDays = _fetchAndFillSkillDays.FindRange(futurePeriod, skill);
			var sum = 0d;
			
			foreach (var workload in skill.WorkloadCollection)
			{
				var quickForecasterWorkloadParams = new QuickForecasterWorkloadParams
				{
					WorkLoad = workload,
					FuturePeriod = futurePeriod,
					SkillDays = skillDays,
					HistoricalPeriod = historicalPeriod
				};
				sum += _quickForecasterWorkload.Execute(quickForecasterWorkloadParams);
			}
			return sum / skill.WorkloadCollection.Count();
		}

		public double ForecastForWorkload(IWorkload workload, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod)
		{
			var skillDays = _fetchAndFillSkillDays.FindRange(futurePeriod, workload.Skill);
			var quickForecasterWorkloadParams = new QuickForecasterWorkloadParams
				{
					WorkLoad = workload,
					FuturePeriod = futurePeriod,
					SkillDays = skillDays,
					HistoricalPeriod = historicalPeriod
				};
			return _quickForecasterWorkload.Execute(quickForecasterWorkloadParams);
		}
	}

	public class QuickForecasterWorkloadParams
	{
		public IWorkload WorkLoad { get; set; }
		public DateOnlyPeriod FuturePeriod { get; set; }
		public ICollection<ISkillDay> SkillDays { get; set; }
		public DateOnlyPeriod HistoricalPeriod { get; set; }
	}
}