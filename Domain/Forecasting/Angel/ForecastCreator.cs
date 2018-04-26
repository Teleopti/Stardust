using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public void CreateForecastForWorkload(DateOnlyPeriod futurePeriod, ForecastWorkloadInput workload, IScenario scenario)
		{
			var skill = _skillRepository.FindSkillsWithAtLeastOneQueueSource()
				.SingleOrDefault(s => s.WorkloadCollection.Any(
					w => w.Id.HasValue && w.Id.Value == workload.WorkloadId));
			_quickForecaster.ForecastWorkloadsWithinSkill(skill, workload, futurePeriod, scenario);
		}
	}
}