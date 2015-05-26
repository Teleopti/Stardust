using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public interface IMailboxRepository
	{
		void Persist(Mailbox mailbox);
		Mailbox Get(Guid id);
		IEnumerable<Mailbox> Get(string route);
		IEnumerable<Mailbox> Get(string[] routes);
	}

	public class EmtpyMailboxRepository : IMailboxRepository
	{
		public void Persist(Mailbox mailbox)
		{
		}

		public Mailbox Get(Guid id)
		{
			return new Mailbox();
		}

		public IEnumerable<Mailbox> Get(string route)
		{
			return Enumerable.Empty<Mailbox>();
		}

		public IEnumerable<Mailbox> Get(string[] routes)
		{
			return Enumerable.Empty<Mailbox>();
		}
	}

	public class Mailbox
	{
		public Guid Id { get; set; }
		public string Route { get; set; }
		public ICollection<Interfaces.MessageBroker.Notification> Notifications { get; set; }

		public Mailbox()
		{
			Notifications = new Collection<Interfaces.MessageBroker.Notification>();
		}

		public void AddNotification(Interfaces.MessageBroker.Notification notification)
		{
			Notifications.Add(notification);
		}

		public IEnumerable<Interfaces.MessageBroker.Notification> PopAll()
		{
			var r = Notifications;
			Notifications = new Collection<Interfaces.MessageBroker.Notification>();
			return r;
		}
	}
}