using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecastForAllSkills : IQuickForecastForAllSkills
	{
		private readonly IQuickForecaster _quickForecaster;
		private readonly ISkillRepository _skillRepository;
		private readonly INow _now;

		public QuickForecastForAllSkills(IQuickForecaster quickForecaster, ISkillRepository skillRepository, INow now)
		{
			_quickForecaster = quickForecaster;
			_skillRepository = skillRepository;
			_now = now;
		}

		public double CreateForecast(DateOnlyPeriod futurePeriod)
		{
			var nowDate = _now.LocalDateOnly();
			var historicalPeriod = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-1)), nowDate);
			_skillRepository.FindSkillsWithAtLeastOneQueueSource().ForEach(skill =>
				_quickForecaster.Execute(skill, futurePeriod, historicalPeriod));
			return 0;
		}
	}
}