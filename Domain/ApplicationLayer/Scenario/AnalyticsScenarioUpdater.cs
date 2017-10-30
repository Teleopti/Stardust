using System;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Scenario
{
	public class AnalyticsScenarioUpdater :
		IHandleEvent<ScenarioChangeEvent>,
		IHandleEvent<ScenarioDeleteEvent>,
		IRunOnHangfire
	{
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsScenarioRepository _analyticsScenarioRepository;
		private readonly IScenarioRepository _scenarioRepository;

		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsScenarioUpdater));

		public AnalyticsScenarioUpdater(IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository, IAnalyticsScenarioRepository analyticsScenarioRepository, IScenarioRepository scenarioRepository)
		{
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsScenarioRepository = analyticsScenarioRepository;
			_scenarioRepository = scenarioRepository;

			logger.Debug($"New instance of {nameof(AnalyticsScenarioUpdater)} was created");
		}

		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(ScenarioDeleteEvent @event)
		{
			logger.Debug($"Consuming {nameof(ScenarioDeleteEvent)} for scenario id = {@event.ScenarioId}. (Message timestamp = {@event.Timestamp})");
			var analyticsScenario = _analyticsScenarioRepository.Get(@event.ScenarioId);

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

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(ScenarioChangeEvent @event)
		{
			logger.Debug($"Consuming {nameof(ScenarioChangeEvent)} for scenario id = {@event.ScenarioId}. (Message timestamp = {@event.Timestamp})");
			var scenario = _scenarioRepository.Load(@event.ScenarioId);
			if (scenario == null)
			{
				logger.Warn("Scenario missing from Application database, aborting.");
				return;
			}
				
			var analyticsBusinessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);
			if (analyticsBusinessUnit == null) throw new BusinessUnitMissingInAnalyticsException();
			var analyticsScenario = _analyticsScenarioRepository.Get(@event.ScenarioId);

			// Add
			if (analyticsScenario == null)
			{
				if (scenario.EnableReporting || scenario.DefaultScenario)
				{
					_analyticsScenarioRepository.AddScenario(transformToAnalyticsScenario(@event, scenario, analyticsBusinessUnit, false));
				}
			}
			// Update
			else
			{
				_analyticsScenarioRepository.UpdateScenario(scenario.EnableReporting || scenario.DefaultScenario
					? transformToAnalyticsScenario(@event, scenario, analyticsBusinessUnit, false)
					: transformToAnalyticsScenario(@event, scenario, analyticsBusinessUnit, true));
			}
		}

		private static AnalyticsScenario transformToAnalyticsScenario(ScenarioChangeEvent @event, IScenario scenario, AnalyticBusinessUnit analyticsBusinessUnit, bool isDeleted)
		{
			return new AnalyticsScenario
			{
				ScenarioCode = @event.ScenarioId,
				ScenarioName = scenario.Description.Name,
				BusinessUnitId = analyticsBusinessUnit.BusinessUnitId,
				BusinessUnitCode = analyticsBusinessUnit.BusinessUnitCode,
				BusinessUnitName = analyticsBusinessUnit.BusinessUnitName,
				DatasourceId = 1,
				DatasourceUpdateDate = scenario.UpdatedOn.GetValueOrDefault(DateTime.UtcNow),
				DefaultScenario = scenario.DefaultScenario,
				IsDeleted = isDeleted
			};
		}
	}
}
