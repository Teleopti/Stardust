using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeMessageSender : IMessageSender
	{
		public FakeMessageSender()
		{
			AllNotifications = new List<Interfaces.MessageBroker.Message>();
		}

		public ICollection<Interfaces.MessageBroker.Message> AllNotifications;

		public void Send(Interfaces.MessageBroker.Message message)
		{
			lock (AllNotifications)
				AllNotifications.Add(message);
		}

		public IEnumerable<Interfaces.MessageBroker.Message> NotificationsOfDomainType<T>()
		{
			return AllNotifications.Where(n => n.DomainType.Equals(typeof(T).Name));
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			throw new NotImplementedException();
		}
	}
}