using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Models;
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

		public IList<ForecastResultModel> CreateForecastForWorkload(DateOnlyPeriod futurePeriod, ForecastWorkloadInput workload, IScenario scenario)
		{
			var skill = _skillRepository.FindSkillsWithAtLeastOneQueueSource()
				.SingleOrDefault(s => s.WorkloadCollection.Any(
					w => w.Id.HasValue && w.Id.Value == workload.WorkloadId));
			return _quickForecaster.ForecastWorkloadsWithinSkill(skill, workload, futurePeriod, scenario);
		}
	}
}