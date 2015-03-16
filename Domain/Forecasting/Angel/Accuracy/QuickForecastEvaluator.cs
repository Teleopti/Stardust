using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class QuickForecastEvaluator : IQuickForecastEvaluator
	{
		private readonly IQuickForecastSkillEvaluator _quickForecastSkillEvaluator;
		private readonly ISkillRepository _skillRepository;
		private readonly INow _now;

		public QuickForecastEvaluator(IQuickForecastSkillEvaluator quickForecastSkillEvaluator, ISkillRepository skillRepository, INow now)
		{
			_quickForecastSkillEvaluator = quickForecastSkillEvaluator;
			_skillRepository = skillRepository;
			_now = now;
		}

		private DateOnlyPeriod getHistoricalPeriod()
		{
			var nowDate = _now.LocalDateOnly();
			var historicalPeriod = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-2)), nowDate);
			return historicalPeriod;
		}

		public ForecastingAccuracy[] MeasureForecastForAllSkills(DateOnlyPeriod futurePeriod)
		{
			var historicalPeriod = getHistoricalPeriod();
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			var list = new List<ForecastingAccuracy>();
			foreach (var skill in skills)
			{
				list.AddRange(_quickForecastSkillEvaluator.Measure(skill, futurePeriod, historicalPeriod));
			}
			return list.ToArray();
		}



	}
}