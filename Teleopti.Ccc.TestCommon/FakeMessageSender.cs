using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeMessageSender : IMessageSender
	{
		public FakeMessageSender()
		{
			AllNotifications = new List<Message>();
		}

		public Message LastMessage;
		public ICollection<Message> AllNotifications;

		public void Send(Message message)
		{
			lock (AllNotifications)
			{
				LastMessage = message;
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