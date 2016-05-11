using System;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
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
		IHandleEvent<ScenarioAddEvent>,
		IRunOnHangfire
	{
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsScenarioRepository _analyticsScenarioRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;

		private readonly static ILog logger = LogManager.GetLogger(typeof(PreferenceChangedHandler));

		public ScenarioChangedHandler(IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository, IAnalyticsScenarioRepository analyticsScenarioRepository, IScenarioRepository scenarioRepository, IBusinessUnitRepository businessUnitRepository)
		{
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsScenarioRepository = analyticsScenarioRepository;
			_scenarioRepository = scenarioRepository;
			_businessUnitRepository = businessUnitRepository;

			logger.Debug($"New instance of {nameof(ScenarioChangedHandler)} was created");
		}
		
		[AnalyticsUnitOfWork]
		public virtual void Handle(ScenarioNameChangeEvent @event)
		{
			logger.Debug($"Consuming {nameof(ScenarioNameChangeEvent)} for scenario id = {@event.ScenarioId}. (Message timestamp = {@event.Timestamp})");
			_analyticsScenarioRepository.SetName(@event.ScenarioId, @event.ScenarioName, @event.LogOnBusinessUnitId);
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public virtual void Handle(ScenarioAddEvent @event)
		{
			logger.Debug($"Consuming {nameof(ScenarioAddEvent)} for scenario id = {@event.ScenarioId}. (Message timestamp = {@event.Timestamp})");
			var scenario = _scenarioRepository.Load(@event.ScenarioId);
			var businessUnit = _businessUnitRepository.Load(@event.LogOnBusinessUnitId);
			var analyticsBusinessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);

			if (scenario == null || businessUnit == null || analyticsBusinessUnit == null)
				return;

			_analyticsScenarioRepository.AddScenario(new AnalyticsScenario
			{
				ScenarioCode = @event.ScenarioId,
				ScenarioName = scenario.Description.Name,
				BusinessUnitId = analyticsBusinessUnit.BusinessUnitId,
				BusinessUnitCode = businessUnit.Id.GetValueOrDefault(),
				BusinessUnitName = businessUnit.Name,
				DatasourceId = 1,
				DatasourceUpdateDate = scenario.UpdatedOn.GetValueOrDefault(DateTime.UtcNow),
				DefaultScenario = scenario.DefaultScenario,
				IsDeleted = false
			});
		}
	}
}
