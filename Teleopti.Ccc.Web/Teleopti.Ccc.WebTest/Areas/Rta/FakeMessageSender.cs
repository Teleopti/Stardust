﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class FakeMessageSender : IMessageSender
	{
		public FakeMessageSender()
		{
			AllNotifications = new List<Notification>();
		}

		public Notification LastNotification;
		public Notification LastTeamNotification;
		public Notification LastSiteNotification;
		public Notification LastAgentsNotification;
		public ICollection<Notification> AllNotifications;

		public void Send(Notification notification)
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

		public Notification NotificationOfType<T>()
		{
			return NotificationsOfType<T>().FirstOrDefault();
		}

		public IEnumerable<Notification> NotificationsOfType<T>()
		{
			return AllNotifications.Where(n => n.DomainType.Equals(typeof(T).Name));
		}

		public void SendMultiple(IEnumerable<Notification> notifications)
		{
			throw new NotImplementedException();
		}
	}
}