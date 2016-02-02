using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class ShiftExchangeOfferHandler : 
		IHandleEvent<ProjectionChangedEvent>,
		IRunOnServiceBus
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPushMessagePersister _msgPersister;

		public ShiftExchangeOfferHandler(IPersonRepository personRepository, IPushMessagePersister msgPersister, IPersonRequestRepository personRequestRepository)
		{
			_personRepository = personRepository;
			_msgPersister = msgPersister;
			_personRequestRepository = personRequestRepository;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
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