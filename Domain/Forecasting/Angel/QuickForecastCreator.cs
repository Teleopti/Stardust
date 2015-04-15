using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecastCreator : IQuickForecastCreator
	{
		private readonly IQuickForecaster _quickForecaster;
		private readonly ISkillRepository _skillRepository;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;

		public QuickForecastCreator(IQuickForecaster quickForecaster, ISkillRepository skillRepository, IHistoricalPeriodProvider historicalPeriodProvider)
		{
			_quickForecaster = quickForecaster;
			_skillRepository = skillRepository;
			_historicalPeriodProvider = historicalPeriodProvider;
		}

		public void CreateForecastForAll(DateOnlyPeriod futurePeriod)
		{
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			skills.ForEach(skill => _quickForecaster.ForecastAll(skill, futurePeriod, _historicalPeriodProvider.PeriodForForecast(), _historicalPeriodProvider.PeriodForEvaluate()));
		}

		public void CreateForecastForWorkloads(DateOnlyPeriod futurePeriod, ForecastWorkloadInput[] workloads)
		{
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			skills.ForEach(skill => _quickForecaster.ForecastWorkloadsWithinSkill(skill, workloads, futurePeriod, _historicalPeriodProvider.PeriodForForecast(), _historicalPeriodProvider.PeriodForEvaluate()));
		}
	}
}