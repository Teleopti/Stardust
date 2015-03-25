using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecastCreator : IQuickForecastCreator
	{
		private readonly IQuickForecaster _quickForecaster;
		private readonly ISkillRepository _skillRepository;
		private readonly INow _now;

		public QuickForecastCreator(IQuickForecaster quickForecaster, ISkillRepository skillRepository, INow now)
		{
			_quickForecaster = quickForecaster;
			_skillRepository = skillRepository;
			_now = now;
		}

		public void CreateForecastForAll(DateOnlyPeriod futurePeriod)
		{
			var historicalPeriod = getHistoricalPeriod();
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			skills.ForEach(skill => _quickForecaster.ForecastAll(skill, futurePeriod, historicalPeriod));
		}

		public void CreateForecastForWorkloads(DateOnlyPeriod futurePeriod, Guid[] workloadIds)
		{
			var historicalPeriod = getHistoricalPeriod();
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			skills.ForEach(skill => _quickForecaster.ForecastWorkloadsWithinSkill(skill, workloadIds, futurePeriod, historicalPeriod));
		}

		private DateOnlyPeriod getHistoricalPeriod()
		{
			var nowDate = _now.LocalDateOnly();
			var historicalPeriod = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-1)), nowDate);
			return historicalPeriod;
		}
	}
}