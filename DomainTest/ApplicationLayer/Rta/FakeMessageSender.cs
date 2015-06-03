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
			AllNotifications = new List<Interfaces.MessageBroker.Message>();
		}

		public Interfaces.MessageBroker.Message LastMessage;
		public Interfaces.MessageBroker.Message LastTeamMessage;
		public Interfaces.MessageBroker.Message LastSiteMessage;
		public Interfaces.MessageBroker.Message LastAgentsMessage;
		public ICollection<Interfaces.MessageBroker.Message> AllNotifications;

		public void Send(Interfaces.MessageBroker.Message message)
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

		public Interfaces.MessageBroker.Message NotificationOfType<T>()
		{
			return NotificationsOfType<T>().FirstOrDefault();
		}

		public IEnumerable<Interfaces.MessageBroker.Message> NotificationsOfType<T>()
		{
			return AllNotifications.Where(n => n.DomainType.Equals(typeof(T).Name));
		}

		public void SendMultiple(IEnumerable<Interfaces.MessageBroker.Message> notifications)
		{
			throw new NotImplementedException();
		}
	}
}