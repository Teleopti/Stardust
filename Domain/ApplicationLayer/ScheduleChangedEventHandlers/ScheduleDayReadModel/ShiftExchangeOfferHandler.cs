using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class ShiftExchangeOfferHandler : IHandleEvent<ProjectionChangedEvent>
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
				if (!projectionChangedEventScheduleDay.NotScheduled)
				{
					var offerPersonRequests = _personRequestRepository.FindByStatus<ShiftExchangeOffer>(person, projectionChangedEventScheduleDay.Date, 0);
					foreach (var offerPersonRequest in offerPersonRequests)
					{
						var offer = offerPersonRequest.Request as IShiftExchangeOffer;
						if (projectionChangedEventScheduleDay.CheckSum != offer.Checksum)
						{
							offer.Status = ShiftExchangeOfferStatus.Invalid;

							SendPushMessageService.CreateConversation(UserTexts.Resources.AnnouncementInvalid,
							string.Format(UserTexts.Resources.AnnouncementInvalidMessage, offer.Date.ToShortDateString()), false)
							.To(new[] { person }).TranslateMessage().AddReplyOption("OK").SendConversation(_msgPersister);
						}
					}
				}
			}
		}

	}
}