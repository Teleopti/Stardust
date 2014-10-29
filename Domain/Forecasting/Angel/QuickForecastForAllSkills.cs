using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecastForAllSkills : IQuickForecastForAllSkills
	{
		private readonly IQuickForecasterSkill _quickForecaster;
		private readonly ISkillRepository _skillRepository;
		private readonly INow _now;

		public QuickForecastForAllSkills(IQuickForecasterSkill quickForecaster, ISkillRepository skillRepository, INow now)
		{
			_quickForecaster = quickForecaster;
			_skillRepository = skillRepository;
			_now = now;
		}

		public void CreateForecast(DateOnlyPeriod futurePeriod)
		{
			var allSkills = _skillRepository.LoadAll();
			var historicalPeriodStartTime = new DateOnly(_now.LocalDateOnly().Date.AddYears(-1));
			var historicalPeriod = new DateOnlyPeriod(historicalPeriodStartTime, new DateOnly(_now.LocalDateOnly()));
			
			foreach (var skill in allSkills.Where(s => s.WorkloadCollection.Any(w => w.QueueSourceCollection.Any())))
			{
				_quickForecaster.Execute(skill, historicalPeriod, futurePeriod);
			}
		}
	}
}