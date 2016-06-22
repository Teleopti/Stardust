using System;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;

		public AnalyticsOvertimeUpdater(IAnalyticsOvertimeRepository analyticsOvertimeRepository, IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository, IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository)
		{
			_analyticsOvertimeRepository = analyticsOvertimeRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			logger.Debug($"New instance of {nameof(AnalyticsOvertimeUpdater)} was created");
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public virtual void Handle(MultiplicatorDefinitionSetCreated @event)
		{
			logger.Debug($"Consuming {nameof(MultiplicatorDefinitionSetCreated)} for event id = {@event.MultiplicatorDefinitionSetId}.");
			handle(@event);
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public virtual void Handle(MultiplicatorDefinitionSetChanged @event)
		{
			logger.Debug($"Consuming {nameof(MultiplicatorDefinitionSetChanged)} for event id = {@event.MultiplicatorDefinitionSetId}.");
			handle(@event);
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
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
			var multiplicatorDefinitionSet = _multiplicatorDefinitionSetRepository.Get(@event.MultiplicatorDefinitionSetId);
			var businessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);

			_analyticsOvertimeRepository.AddOrUpdate(new AnalyticsOvertime
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				OvertimeCode = @event.MultiplicatorDefinitionSetId,
				OvertimeName = @multiplicatorDefinitionSet.Name,
				DatasourceUpdateDate = multiplicatorDefinitionSet.UpdatedOn ?? DateTime.UtcNow,
				IsDeleted = multiplicatorDefinitionSet.IsDeleted
			});
		}
	}
}