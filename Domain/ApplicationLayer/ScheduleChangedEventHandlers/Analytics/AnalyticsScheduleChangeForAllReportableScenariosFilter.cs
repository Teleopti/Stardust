using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsScheduleChangeForAllReportableScenariosFilter : IAnalyticsScheduleChangeUpdaterFilter
	{
		private readonly IScenarioRepository _scenarioRepository;

		public AnalyticsScheduleChangeForAllReportableScenariosFilter(IScenarioRepository scenarioRepository)
		{
			_scenarioRepository = scenarioRepository;
		}

		public bool ContinueProcessingEvent(ProjectionChangedEvent @event)
		{
			if (@event.IsDefaultScenario)
				return true;

			return _scenarioRepository.FindEnabledForReportingSorted().Any(a => a.Id.Equals(@event.ScenarioId));
		}
	}
}