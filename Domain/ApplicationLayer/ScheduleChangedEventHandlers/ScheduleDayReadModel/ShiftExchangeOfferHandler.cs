using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class ShiftExchangeOfferHandler : IHandleEvent<ProjectionChangedEvent>
	{
		private readonly IPersonRepository _personRepository;
		private readonly IShiftExchangeOfferRepository _shiftExchangeOfferRepository;
		private readonly IPushMessagePersister _msgPersister;

		public ShiftExchangeOfferHandler(IPersonRepository personRepository, IShiftExchangeOfferRepository shiftExchangeOfferRepository, IPushMessagePersister msgPersister)
		{
			_personRepository = personRepository;
			_shiftExchangeOfferRepository = shiftExchangeOfferRepository;
			_msgPersister = msgPersister;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			var person = _personRepository.Get(@event.PersonId);
			foreach (var projectionChangedEventScheduleDay in @event.ScheduleDays)
			{
				if (!projectionChangedEventScheduleDay.NotScheduled)
				{
					var offers = _shiftExchangeOfferRepository.FindPendingOffer(person, new DateOnly(projectionChangedEventScheduleDay.Date));
					foreach (var offer in offers)
					{
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