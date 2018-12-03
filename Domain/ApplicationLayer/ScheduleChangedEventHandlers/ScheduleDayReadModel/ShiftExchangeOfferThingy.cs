using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class ShiftExchangeOfferThingy
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPushMessagePersister _msgPersister;
		private readonly IPersonRequestRepository _personRequestRepository;

		public ShiftExchangeOfferThingy(IPersonRepository personRepository, 
			IPushMessagePersister msgPersister, 
			IPersonRequestRepository personRequestRepository)
		{
			_personRepository = personRepository;
			_msgPersister = msgPersister;
			_personRequestRepository = personRequestRepository;
		}
		
		public void Execute(Guid personId, IEnumerable<(DateTime Date, long CheckSum)> dateAndChecksums)
		{
			var person = _personRepository.Get(personId);
			foreach (var projectionChangedEventScheduleDay in dateAndChecksums)
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
							.To(new[] {person}).TranslateMessage().AddReplyOption("OK").SendConversation(_msgPersister);
					}
				}
			}
		}
	}
}