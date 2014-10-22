using System.Linq;
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

		public void CreateForecast(DateOnlyPeriod futurePeriod)
		{
			var allSkills = _skillRepository.LoadAll();
			var historicalPeriodStartTime = new DateOnly(_now.UtcDateTime().AddYears(-1));
			var historicalPeriod = new DateOnlyPeriod(historicalPeriodStartTime, new DateOnly(_now.UtcDateTime()));

			foreach (var workload in allSkills.SelectMany(skill => skill.WorkloadCollection))
			{
				_quickForecaster.Execute(workload, historicalPeriod, futurePeriod);
			}
		}
	}
}