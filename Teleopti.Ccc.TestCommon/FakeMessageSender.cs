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

		public ICollection<Message> AllNotifications;

		public void Send(Message message)
		{
			lock (AllNotifications)
				AllNotifications.Add(message);
		}

		public IEnumerable<Message> NotificationsOfDomainType<T>()
		{
			return AllNotifications.Where(n => n.DomainType.Equals(typeof(T).Name));
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			messages.ForEach(AllNotifications.Add);
		}
	}
}