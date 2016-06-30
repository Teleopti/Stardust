using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.BusinessUnit
{
	[EnabledBy(Toggles.ETL_SpeedUpIntradayBusinessUnit_38932)]
	public class AnalyticsBusinessUnitUpdater : 
		IHandleEvent<BusinessUnitChangedEvent>, 
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsBusinessUnitUpdater));

		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;

		public AnalyticsBusinessUnitUpdater(IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository)
		{
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
		}

		[AnalyticsUnitOfWork]
		public virtual void Handle(BusinessUnitChangedEvent @event)
		{
			logger.Info($"Consuming {nameof(BusinessUnitChangedEvent)} for BusinessUnit {@event.BusinessUnitName} ({@event.BusinessUnitId})");
			_analyticsBusinessUnitRepository.AddOrUpdate(new AnalyticBusinessUnit
			{
				BusinessUnitCode = @event.BusinessUnitId,
				BusinessUnitName = @event.BusinessUnitName,
				DatasourceUpdateDate = @event.UpdatedOn
			});
		}
	}
}