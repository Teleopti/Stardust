using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class QuickForecastEvaluator : IQuickForecastEvaluator
	{
		private readonly IQuickForecastSkillEvaluator _quickForecastSkillEvaluator;
		private readonly ISkillRepository _skillRepository;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;

		public QuickForecastEvaluator(IQuickForecastSkillEvaluator quickForecastSkillEvaluator, ISkillRepository skillRepository, IHistoricalPeriodProvider historicalPeriodProvider)
		{
			_quickForecastSkillEvaluator = quickForecastSkillEvaluator;
			_skillRepository = skillRepository;
			_historicalPeriodProvider = historicalPeriodProvider;
		}

		public IEnumerable<SkillAccuracy> MeasureForecastForAllSkills()
		{
			var historicalPeriod = _historicalPeriodProvider.PeriodForEvaluate();
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			return skills.Select(skill => _quickForecastSkillEvaluator.Measure(skill, historicalPeriod)).ToList();
		}
	}
}