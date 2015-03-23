using System;
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

		public void ForecastWorkloadsWithinSkill(ISkill skill, Guid[] workloadIds, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod)
		{
			var skillDays = _fetchAndFillSkillDays.FindRange(futurePeriod, skill);
			
			foreach (var workload in skill.WorkloadCollection)
			{
				if (workloadIds.Contains(workload.Id.Value))
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
	}

	public struct QuickForecasterWorkloadParams
	{
		public IWorkload WorkLoad { get; set; }
		public DateOnlyPeriod FuturePeriod { get; set; }
		public ICollection<ISkillDay> SkillDays { get; set; }
		public DateOnlyPeriod HistoricalPeriod { get; set; }
	}
}