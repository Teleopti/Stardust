using System.Linq;
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
			var allSkills = _skillRepository.LoadAll();
			
			foreach (var skill in allSkills.Where(s => s.WorkloadCollection.Any(w => w.QueueSourceCollection.Any())))
			{
				_quickForecaster.Execute(skill, historicalPeriod, futurePeriod);
			}
		}
	}
}