using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Preference;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Scenario
{
	[EnabledBy(Toggles.ETL_SpeedUpScenario_38300)]
	public class ScenarioChangedHandler :
		IHandleEvent<ScenarioChangeEvent>,
		IHandleEvent<ScenarioDeleteEvent>,
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
		public virtual void Handle(ScenarioDeleteEvent @event)
		{
			logger.Debug($"Consuming {nameof(ScenarioDeleteEvent)} for scenario id = {@event.ScenarioId}. (Message timestamp = {@event.Timestamp})");
			var analyticsScenario = _analyticsScenarioRepository.Scenarios().FirstOrDefault(a => a.ScenarioCode == @event.ScenarioId);

			if (analyticsScenario == null)
				return;

			// Delete
			_analyticsScenarioRepository.UpdateScenario(new AnalyticsScenario
			{
				ScenarioCode = @event.ScenarioId,
				ScenarioName = analyticsScenario.ScenarioName,
				BusinessUnitId = analyticsScenario.BusinessUnitId,
				BusinessUnitCode = analyticsScenario.BusinessUnitCode,
				BusinessUnitName = analyticsScenario.BusinessUnitName,
				DatasourceId = analyticsScenario.DatasourceId,
				DatasourceUpdateDate = analyticsScenario.DatasourceUpdateDate,
				DefaultScenario = analyticsScenario.DefaultScenario,
				IsDeleted = true
			});
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public virtual void Handle(ScenarioChangeEvent @event)
		{
			logger.Debug($"Consuming {nameof(ScenarioChangeEvent)} for scenario id = {@event.ScenarioId}. (Message timestamp = {@event.Timestamp})");
			var scenario = _scenarioRepository.Load(@event.ScenarioId);
			var businessUnit = _businessUnitRepository.Load(@event.LogOnBusinessUnitId);
			var analyticsBusinessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);
			var analyticsScenario = _analyticsScenarioRepository.Scenarios().FirstOrDefault(a => a.ScenarioCode == @event.ScenarioId);

			if (scenario == null || businessUnit == null || analyticsBusinessUnit == null)
				return;

			// Add
			if (analyticsScenario == null)
			{
				_analyticsScenarioRepository.AddScenario(transformToAnalyticsScenario(@event, scenario, businessUnit, analyticsBusinessUnit));
			}
			// Update
			else
			{
				_analyticsScenarioRepository.UpdateScenario(transformToAnalyticsScenario(@event, scenario, businessUnit, analyticsBusinessUnit));
			}
		}

		private static AnalyticsScenario transformToAnalyticsScenario(ScenarioChangeEvent @event, IScenario scenario, IBusinessUnit businessUnit, AnalyticBusinessUnit analyticsBusinessUnit)
		{
			return new AnalyticsScenario
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
			};
		}
	}
}
