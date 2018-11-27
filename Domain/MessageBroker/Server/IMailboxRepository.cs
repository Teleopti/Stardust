using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public interface IMailboxRepository
	{
		void Add(Mailbox mailbox);
		Mailbox Load(Guid id);
		IEnumerable<Message> PopMessages(Guid id, DateTime? expiredAt);
		void AddMessage(Message message);
		[RemoveMeWithToggle(Toggles.MessageBroker_VeganBurger_79140)]
		void Purge();
		void PurgeOneChunkOfMailboxes();
		void PurgeOneChunkOfNotifications();
	}
}