using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastCreator : IForecastCreator
	{
		private readonly IQuickForecaster _quickForecaster;
		private readonly ISkillRepository _skillRepository;

		public ForecastCreator(IQuickForecaster quickForecaster, ISkillRepository skillRepository)
		{
			_quickForecaster = quickForecaster;
			_skillRepository = skillRepository;
		}

		public void CreateForecastForWorkloads(DateOnlyPeriod futurePeriod, ForecastWorkloadInput[] workloads, IScenario scenario)
		{
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			skills.ForEach(skill => _quickForecaster.ForecastWorkloadsWithinSkill(skill, workloads, futurePeriod, scenario));
		}
	}
}