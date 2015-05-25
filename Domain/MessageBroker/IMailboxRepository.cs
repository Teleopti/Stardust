using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public interface IMailboxRepository
	{
		void Persist(Mailbox mailbox);
		Mailbox Get(Guid id);
		IEnumerable<Mailbox> Get(string route);
	}

	public class Mailbox
	{
		public Guid Id { get; set; }
		public string Route { get; set; }
		public IEnumerable<Interfaces.MessageBroker.Notification> Notifications { get; set; }

		public Mailbox()
		{
			Notifications = new Interfaces.MessageBroker.Notification[] { };
		}

		public void AddNotification(Interfaces.MessageBroker.Notification notification)
		{
			Notifications = Notifications.Concat(new[] { notification });
		}

		public IEnumerable<Interfaces.MessageBroker.Notification> PopAll()
		{
			var r = Notifications;
			Notifications = new Interfaces.MessageBroker.Notification[] { };
			return r;
		}
	}
}