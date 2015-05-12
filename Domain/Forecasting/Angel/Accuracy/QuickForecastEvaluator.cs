using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class QuickForecastEvaluator : IQuickForecastEvaluator
	{
		private readonly IQuickForecastSkillEvaluator _quickForecastSkillEvaluator;
		private readonly ISkillRepository _skillRepository;

		public QuickForecastEvaluator(IQuickForecastSkillEvaluator quickForecastSkillEvaluator, ISkillRepository skillRepository)
		{
			_quickForecastSkillEvaluator = quickForecastSkillEvaluator;
			_skillRepository = skillRepository;
		}

		public IEnumerable<SkillAccuracy> MeasureForecastForAllSkills()
		{
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			return skills.Select(skill => _quickForecastSkillEvaluator.Measure(skill)).ToList();
		}
	}
}