using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeMessageSender : IMessageSender
	{
		public FakeMessageSender()
		{
			AllNotifications = new List<Message>();
		}

		public Message LastMessage;
		public Message LastTeamMessage;
		public Message LastSiteMessage;
		public Message LastAgentsMessage;
		public ICollection<Message> AllNotifications;

		public void Send(Message message)
		{
			lock (AllNotifications)
			{
				LastMessage = message;

				if (message.DomainType.Contains("Team"))
					LastTeamMessage = message;
				else if (message.DomainType.Contains("Agents"))
					LastAgentsMessage = message;
				else
					LastSiteMessage = message;
				lock (AllNotifications)
					AllNotifications.Add(message);
			}
		}

		public IEnumerable<Message> NotificationsOfDomainType<T>()
		{
			return AllNotifications.Where(n => n.DomainType.Equals(typeof(T).Name));
		}

		public Message NotificationOfType<T>()
		{
			return NotificationsOfType<T>().FirstOrDefault();
		}

		public IEnumerable<Message> NotificationsOfType<T>()
		{
			return AllNotifications.Where(n => n.DomainType.Equals(typeof(T).Name));
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			messages.ForEach(AllNotifications.Add);
		}
	}
}