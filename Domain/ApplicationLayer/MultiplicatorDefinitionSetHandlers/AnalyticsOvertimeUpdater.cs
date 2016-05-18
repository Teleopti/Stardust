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
		private readonly IAnalyticsOvertimeRepository _analyticsOvertimeRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;

		public AnalyticsOvertimeUpdater(IAnalyticsOvertimeRepository analyticsOvertimeRepository, IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository)
		{
			_analyticsOvertimeRepository = analyticsOvertimeRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
		}

		[AnalyticsUnitOfWork]
		public virtual void Handle(MultiplicatorDefinitionSetCreated @event)
		{
			handle(@event);
		}

		[AnalyticsUnitOfWork]
		public virtual void Handle(MultiplicatorDefinitionSetChanged @event)
		{
			handle(@event);
		}

		[AnalyticsUnitOfWork]
		public virtual void Handle(MultiplicatorDefinitionSetDeleted @event)
		{
			handle(@event);
		}

		private void handle(MultiplicatorDefinitionSetChangedBase @event)
		{
			if (@event.MultiplicatorType != MultiplicatorType.Overtime) return;

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