using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecastCreator : IQuickForecastCreator
	{
		private readonly IQuickForecaster _quickForecaster;
		private readonly ISkillRepository _skillRepository;

		public QuickForecastCreator(IQuickForecaster quickForecaster, ISkillRepository skillRepository)
		{
			_quickForecaster = quickForecaster;
			_skillRepository = skillRepository;
		}

		public void CreateForecastForAll(DateOnlyPeriod futurePeriod)
		{
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			skills.ForEach(skill => _quickForecaster.ForecastAll(skill, futurePeriod));
		}

		public void CreateForecastForWorkloads(DateOnlyPeriod futurePeriod, ForecastWorkloadInput[] workloads)
		{
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			skills.ForEach(skill => _quickForecaster.ForecastWorkloadsWithinSkill(skill, workloads, futurePeriod));
		}
	}
}