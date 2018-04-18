using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class FetchAnalyticsScenarios
	{
		private readonly IAnalyticsScenarioRepository _analyticsScenarioRepository;

		public FetchAnalyticsScenarios(IAnalyticsScenarioRepository analyticsScenarioRepository)
		{
			_analyticsScenarioRepository = analyticsScenarioRepository;
		}
		
		public IEnumerable<AnalyticsScenario> Execute()
		{
			var scenarios = _analyticsScenarioRepository.Scenarios();
			return scenarios.Where(x => !x.IsDeleted && x.ScenarioId != -1).ToList();
		}
	}
}