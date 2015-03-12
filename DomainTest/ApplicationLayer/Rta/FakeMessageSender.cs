using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class FakeMessageSender : IMessageSender
	{
		public FakeMessageSender()
		{
			AllNotifications = new List<Interfaces.MessageBroker.Notification>();
		}

		public Interfaces.MessageBroker.Notification LastNotification;
		public Interfaces.MessageBroker.Notification LastTeamNotification;
		public Interfaces.MessageBroker.Notification LastSiteNotification;
		public Interfaces.MessageBroker.Notification LastAgentsNotification;
		public ICollection<Interfaces.MessageBroker.Notification> AllNotifications;

		public void Send(Interfaces.MessageBroker.Notification notification)
		{
			LastNotification = notification;
			
			if (notification.DomainType.Contains("Team"))
				LastTeamNotification = notification;
			else if (notification.DomainType.Contains("Agents"))
				LastAgentsNotification = notification;
			else
				LastSiteNotification = notification;
			AllNotifications.Add(notification);
		}

		public Interfaces.MessageBroker.Notification NotificationOfType<T>()
		{
			return NotificationsOfType<T>().FirstOrDefault();
		}

		public IEnumerable<Interfaces.MessageBroker.Notification> NotificationsOfType<T>()
		{
			return AllNotifications.Where(n => n.DomainType.Equals(typeof(T).Name));
		}

		public void SendMultiple(IEnumerable<Interfaces.MessageBroker.Notification> notifications)
		{
			throw new NotImplementedException();
		}
	}
}