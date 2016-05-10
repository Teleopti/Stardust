using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Preference;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Scenario
{
	[UseOnToggle(Toggles.ETL_SpeedUpScenario_38300)]
	public class ScenarioChangedHandler :
		IHandleEvent<ScenarioNameChangeEvent>,
		IHandleEvent<ScenarioReportableChangeEvent>,
		IHandleEvent<ScenarioAddEvent>,
		IRunOnHangfire
	{
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;

		private readonly static ILog logger = LogManager.GetLogger(typeof(PreferenceChangedHandler));

		public ScenarioChangedHandler(IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository)
		{
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;

			logger.Info("New instance of handler was created");
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		public void Handle(ScenarioNameChangeEvent @event)
		{
			logger.Debug($"Consuming event for scenario id = {@event.ScenarioId}. (Message timestamp = {@event.Timestamp})");
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		public void Handle(ScenarioReportableChangeEvent @event)
		{
			logger.Debug($"Consuming event for scenario id = {@event.ScenarioId}. (Message timestamp = {@event.Timestamp})");
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		public void Handle(ScenarioAddEvent @event)
		{
			throw new System.NotImplementedException();
		}
	}
}
