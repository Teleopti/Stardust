using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Hubs;
using log4net;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Ccc.Web.Broker
{
	[HubName("MessageBrokerHub")]
	public class MessageBrokerHub : BackportableHub
	{
		public ILog Logger = LogManager.GetLogger(typeof(MessageBrokerHub));

		private IActionScheduler _actionScheduler;
		private readonly IMessageBrokerServer _server;

		public MessageBrokerHub(IActionScheduler actionScheduler, IMessageBrokerServer server)
		{
			_actionScheduler = actionScheduler;
			_server = server;
		}

		public string AddSubscription(Subscription subscription)
		{
			return _server.AddSubscription(subscription, Context.ConnectionId);
		}

		public void RemoveSubscription(string route)
		{
			_server.RemoveSubscription(route, Context.ConnectionId);
		}

		public void NotifyClients(Message message)
		{
			_server.NotifyClients(message);
		}

		public void NotifyClientsMultiple(IEnumerable<Message> notifications)
		{
			_server.NotifyClientsMultiple(notifications);
		}

		public void Ping()
		{
			Clients.Caller.Pong();
		}

		public void PingWithId(double id)
		{
			Clients.Caller.Pong(id);
		}

		public void Ping(int expectedNumberOfSentMessages)
		{
			for (var i = 0; i < expectedNumberOfSentMessages; i++)
			{
				_actionScheduler.Do(Ping);
			}
		}

		public void Ping(int expectedNumberOfSentMessages, int messagesPerSecond)
		{
			_actionScheduler = new ActionThrottle(messagesPerSecond);
			((ActionThrottle)_actionScheduler).Start();

			Ping(expectedNumberOfSentMessages);
		}
	}

}