using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.MultiplicatorDefinitionSetHandlers
{
	[EnabledBy(Toggles.ETL_SpeedUpIntradayOvertime_38304)]
	public class AnalyticsOvertimeUpdater : 
		IRunOnHangfire,
		IHandleEvent<MultiplicatorDefinitionSetCreated>,
		IHandleEvent<MultiplicatorDefinitionSetChanged>,
		IHandleEvent<MultiplicatorDefinitionSetDeleted>
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(AnalyticsOvertimeUpdater));

		private readonly IAnalyticsOvertimeRepository _analyticsOvertimeRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;

		public AnalyticsOvertimeUpdater(IAnalyticsOvertimeRepository analyticsOvertimeRepository, IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository)
		{
			_analyticsOvertimeRepository = analyticsOvertimeRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			logger.Debug($"New instance of {nameof(AnalyticsOvertimeUpdater)} was created");
		}

		[AnalyticsUnitOfWork]
		public virtual void Handle(MultiplicatorDefinitionSetCreated @event)
		{
			logger.Debug($"Consuming {nameof(MultiplicatorDefinitionSetCreated)} for event id = {@event.MultiplicatorDefinitionSetId}.");
			handle(@event);
		}

		[AnalyticsUnitOfWork]
		public virtual void Handle(MultiplicatorDefinitionSetChanged @event)
		{
			logger.Debug($"Consuming {nameof(MultiplicatorDefinitionSetChanged)} for event id = {@event.MultiplicatorDefinitionSetId}.");
			handle(@event);
		}

		[AnalyticsUnitOfWork]
		public virtual void Handle(MultiplicatorDefinitionSetDeleted @event)
		{
			logger.Debug($"Consuming {nameof(MultiplicatorDefinitionSetDeleted)} for event id = {@event.MultiplicatorDefinitionSetId}.");
			handle(@event);
		}

		private void handle(MultiplicatorDefinitionSetChangedBase @event)
		{

			if (@event.MultiplicatorType != MultiplicatorType.Overtime)
			{
				logger.Debug($"Not {nameof(MultiplicatorType.Overtime)}, ending processing.");
				return;
			}

			var businessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);

			_analyticsOvertimeRepository.AddOrUpdate(new AnalyticsOvertime
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				OvertimeCode = @event.MultiplicatorDefinitionSetId,
				OvertimeName = @event.MultiplicatorDefinitionSetName,
				DatasourceUpdateDate = @event.DatasourceUpdateDate,
				IsDeleted = @event.IsDeleted
			});
		}
	}
}