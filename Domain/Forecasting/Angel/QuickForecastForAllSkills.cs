using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecastForAllSkills : IQuickForecastForAllSkills
	{
		private readonly IQuickForecasterWorkload _quickForecasterWorkload;
		private readonly ISkillRepository _skillRepository;
		private readonly INow _now;

		public QuickForecastForAllSkills(IQuickForecasterWorkload quickForecasterWorkload, ISkillRepository skillRepository, INow now)
		{
			_quickForecasterWorkload = quickForecasterWorkload;
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
				_quickForecasterWorkload.Execute(workload, historicalPeriod, futurePeriod);
			}
		}
	}
}