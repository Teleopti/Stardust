using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	[EnabledBy(Toggles.ETL_FixScheduleForPersonPeriod_41393)]
	public class AnalyticsPreferenceMatchingPerson : IHandleEvent<AnalyticsPersonPeriodRangeChangedEvent>, IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsPreferenceMatchingPerson));
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsPreferenceRepository _analyticsPreferenceRepository;

		public AnalyticsPreferenceMatchingPerson(IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository, IAnalyticsPreferenceRepository analyticsPreferenceRepository)
		{
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsPreferenceRepository = analyticsPreferenceRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork, AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(AnalyticsPersonPeriodRangeChangedEvent @event)
		{
			logger.Debug($"Handle {nameof(AnalyticsPersonPeriodRangeChangedEvent)} for {@event.SerializedPeople}");
			foreach (var personId in @event.PersonIdCollection)
			{
				var analyticsPersonPeriods = _analyticsPersonPeriodRepository.GetPersonPeriods(personId);
				var personPeriodIds = analyticsPersonPeriods.Select(x => x.PersonId).ToArray();
				_analyticsPreferenceRepository.UpdateUnlinkedPersonids(personPeriodIds);
			}
		}
	}
}