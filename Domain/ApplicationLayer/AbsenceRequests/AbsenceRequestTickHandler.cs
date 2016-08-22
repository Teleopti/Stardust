using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	[EnabledBy(Toggles.AbsenceRequests_UseMultiRequestProcessing_39960)]
	public class AbsenceRequestTickHandler : IHandleEvent<TenantMinuteTickEvent>, IRunOnHangfire
	{
		private readonly IAbsenceRequestStrategyProcessor _absenceRequestStrategyProcessor;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IEventPublisher _publisher;
		private MutableNow _now;


		public AbsenceRequestTickHandler(IAbsenceRequestStrategyProcessor absenceRequestStrategyProcessor,
			IEventPublisher publisher, MutableNow now, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository)
		{
			_absenceRequestStrategyProcessor = absenceRequestStrategyProcessor;
			_publisher = publisher;
			_now = now;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
		}

		[UnitOfWork]
		public void Handle(TenantMinuteTickEvent @event)
		{
			var now = _now.UtcDateTime();
			var nearFuture = 3;

			var absenceRequests = _absenceRequestStrategyProcessor.Get(_now.UtcDateTime(),
				new DateTimePeriod(now.Date, now.Date.AddDays(nearFuture)));
			var multiAbsenceRequestsEvent = new NewMultiAbsenceRequestsCreatedEvent()
			{
				PersonRequestIds = absenceRequests.ToList()
			};
			_publisher.Publish(multiAbsenceRequestsEvent);
			_queuedAbsenceRequestRepository.Remove(absenceRequests);
		}
	}

}
