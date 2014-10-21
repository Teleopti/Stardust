﻿using System.Collections.Generic;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.WebTest.Areas.Rta.ImplementationDetailsTests.Adherence
{
	public class MessageSenderExposingNotifications : IMessageSender
	{
		public MessageSenderExposingNotifications()
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

		public void SendMultiple(IEnumerable<Notification> notifications)
		{
			throw new System.NotImplementedException();
		}
	}
}