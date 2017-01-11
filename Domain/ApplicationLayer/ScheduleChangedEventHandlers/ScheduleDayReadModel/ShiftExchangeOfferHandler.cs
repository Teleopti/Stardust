using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	[EnabledBy(Toggles.LastHandlers_ToHangfire_41203)]
	public class ShiftExchangeOfferHandlerHangfire :
		ShiftExchangeOfferHandlerBase,
		IHandleEvent<ProjectionChangedEvent>,
		IRunOnHangfire
	{
		public ShiftExchangeOfferHandlerHangfire(IPersonRepository personRepository, IPushMessagePersister msgPersister, IPersonRequestRepository personRequestRepository) : base(personRepository, msgPersister, personRequestRepository)
		{
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			HandleEvent(@event);
		}
	}

	[DisabledBy(Toggles.LastHandlers_ToHangfire_41203)]
	public class ShiftExchangeOfferHandlerServiceBus :
		ShiftExchangeOfferHandlerBase,
		IHandleEvent<ProjectionChangedEvent>,
#pragma warning disable 618
		IRunOnServiceBus
#pragma warning restore 618
	{
		public ShiftExchangeOfferHandlerServiceBus(IPersonRepository personRepository, IPushMessagePersister msgPersister, IPersonRequestRepository personRequestRepository) : base(personRepository, msgPersister, personRequestRepository)
		{
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			HandleEvent(@event);
		}
	}

	public class ShiftExchangeOfferHandlerBase
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPushMessagePersister _msgPersister;

		public ShiftExchangeOfferHandlerBase(IPersonRepository personRepository, IPushMessagePersister msgPersister, IPersonRequestRepository personRequestRepository)
		{
			_personRepository = personRepository;
			_msgPersister = msgPersister;
			_personRequestRepository = personRequestRepository;
		}

		protected void HandleEvent(ProjectionChangedEvent @event)
		{
			if (!@event.IsDefaultScenario)
				return;

			var person = _personRepository.Get(@event.PersonId);
			foreach (var projectionChangedEventScheduleDay in @event.ScheduleDays)
			{
				var date = new DateOnly(projectionChangedEventScheduleDay.Date);
				var offers = _personRequestRepository.FindOfferByStatus(person, date, ShiftExchangeOfferStatus.Pending);
				foreach (var offer in offers)
				{
					if (projectionChangedEventScheduleDay.CheckSum != offer.Checksum)
					{
						offer.Status = ShiftExchangeOfferStatus.Invalid;

						SendPushMessageService.CreateConversation(Resources.AnnouncementInvalid,
						string.Format(Resources.AnnouncementInvalidMessage, offer.Date.ToShortDateString()), false)
						.To(new[] { person }).TranslateMessage().AddReplyOption("OK").SendConversation(_msgPersister);
					}
				}
			}
		}

	}
}