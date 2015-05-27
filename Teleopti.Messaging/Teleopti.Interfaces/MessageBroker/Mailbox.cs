using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.MessageBroker
{
	public class Mailbox
	{
		public virtual Guid Id { get; set; }
		public virtual string Route { get; set; }
		public virtual ICollection<Notification> Notifications { get; set; }

		public Mailbox()
		{
			Notifications = new Collection<Notification>();
		}

		public virtual void AddNotification(Notification notification)
		{
			notification.MailboxParent = this;
			Notifications.Add(notification);
		}

		public virtual IEnumerable<Notification> PopAll()
		{
			var r = Notifications;
			Notifications = new Collection<Notification>();
			return r;
		}
	}
}