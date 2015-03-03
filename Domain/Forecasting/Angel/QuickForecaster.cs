﻿using System.Collections.Generic;
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

		public double Execute(ISkill skill, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod)
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
				_quickForecasterWorkload.Execute(quickForecasterWorkloadParams);
				sum += quickForecasterWorkloadParams.Difference;
			}
			return sum;
		}
	}

	public class QuickForecasterWorkloadParams
	{
		public IWorkload WorkLoad { get; set; }
		public DateOnlyPeriod FuturePeriod { get; set; }
		public ICollection<ISkillDay> SkillDays { get; set; }
		public double Difference { get; set; }
		public DateOnlyPeriod HistoricalPeriod { get; set; }
	}
}