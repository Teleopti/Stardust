using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.MessageBroker
{
	public class Mailbox
	{
		public Guid Id { get; set; }
		public string Route { get; set; }
		public ICollection<Message> Notifications { get; set; }

		public Mailbox()
		{
			Notifications = new Collection<Message>();
		}

		public void AddNotification(Message message)
		{
			Notifications.Add(message);
		}

		public IEnumerable<Message> PopAll()
		{
			var r = Notifications;
			Notifications = new Collection<Message>();
			return r;
		}
	}
}