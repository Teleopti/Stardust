using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecastForAllSkills : IQuickForecastForAllSkills
	{
		private readonly IQuickForecaster _quickForecaster;
		private readonly ISkillRepository _skillRepository;

		public QuickForecastForAllSkills(IQuickForecaster quickForecaster, ISkillRepository skillRepository)
		{
			_quickForecaster = quickForecaster;
			_skillRepository = skillRepository;
		}

		public void CreateForecast(DateOnlyPeriod historicalPeriod, DateOnlyPeriod futurePeriod)
		{
			_skillRepository.FindSkillsWithAtLeastOneQueueSource().ForEach(skill =>
				_quickForecaster.Execute(skill, historicalPeriod, futurePeriod));
		}

		public double MeasureForecast(DateOnlyPeriod historicalPeriod, DateOnlyPeriod futurePeriod)
		{
			throw new System.NotImplementedException();
		}
	}
}