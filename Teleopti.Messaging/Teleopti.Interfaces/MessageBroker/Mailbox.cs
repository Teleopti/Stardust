using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.MessageBroker
{
	public class Mailbox
	{
		public Guid Id { get; set; }
		public string Route { get; set; }
		public ICollection<Notification> Notifications { get; set; }

		public Mailbox()
		{
			Notifications = new Collection<Notification>();
		}

		public void AddNotification(Notification notification)
		{
			Notifications.Add(notification);
		}

		public IEnumerable<Notification> PopAll()
		{
			var r = Notifications;
			Notifications = new Collection<Notification>();
			return r;
		}
	}
}