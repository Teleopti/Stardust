using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Dates
{
	public class AnalyticsPersonLocalDateUpdater : IHandleEvent<AnalyticsDatesChangedEvent>, IRunOnHangfire
	{
		private readonly IAnalyticsDateRepository _analyticsDateRepository;
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;

		public AnalyticsPersonLocalDateUpdater(IAnalyticsDateRepository analyticsDateRepository, IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository)
		{
			_analyticsDateRepository = analyticsDateRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
		}

		[Attempts(10)]
		[AnalyticsUnitOfWork]
		public virtual void Handle(AnalyticsDatesChangedEvent @event)
		{
			var maxDate = _analyticsDateRepository.MaxDate();
			_analyticsPersonPeriodRepository.UpdateValidToLocalDateIds(maxDate);
		}
	}
}