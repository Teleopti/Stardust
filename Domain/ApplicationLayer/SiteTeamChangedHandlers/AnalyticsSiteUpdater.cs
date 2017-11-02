using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SiteTeamChangedHandlers
{
	
	public class AnalyticsSiteUpdater : IHandleEvent<SiteNameChangedEvent>, IRunOnHangfire
	{
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsSiteRepository _analyticsSiteRepository;

		public AnalyticsSiteUpdater(IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository, IAnalyticsSiteRepository analyticsSiteRepository)
		{
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsSiteRepository = analyticsSiteRepository;
		}

		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(SiteNameChangedEvent @event)
		{
			_analyticsSiteRepository.UpdateName(@event.SiteId, @event.Name);
			_analyticsPersonPeriodRepository.UpdateSiteName(@event.SiteId, @event.Name);
		}
	}
}
