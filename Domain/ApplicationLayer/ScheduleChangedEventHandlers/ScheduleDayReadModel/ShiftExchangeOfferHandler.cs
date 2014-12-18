using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
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
			
			var firstTime = true;
			foreach (var projectionChangedEventScheduleDay in @event.ScheduleDays)
			{
				if (firstTime)
				{
					firstTime = false;
					continue;
				}
				if (!projectionChangedEventScheduleDay.NotScheduled)
				{
					var person = _personRepository.Get(@event.PersonId);
					var offers = _shiftExchangeOfferRepository.FindPendingOffer(person,
						new DateOnly(projectionChangedEventScheduleDay.Date));
					offers.ForEach(x =>
					{
						x.Status = ShiftExchangeOfferStatus.Invalid;
						SendPushMessageService.CreateConversation(UserTexts.Resources.AnnouncementInvalid, string.Format(UserTexts.Resources.AnnouncementInvalidMessage, x.Date), false)
							.To(new[] { person }).TranslateMessage().AddReplyOption("OK").SendConversation(_msgPersister);
					});
				}
			}
		}

	}
}