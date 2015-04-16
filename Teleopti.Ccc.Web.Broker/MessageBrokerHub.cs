using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Hubs;
using log4net;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[HubName("MessageBrokerHub")]
	public class MessageBrokerHub : BackportableHub
	{
		public ILog Logger = LogManager.GetLogger(typeof(MessageBrokerHub));

		private IActionScheduler _actionScheduler;
		private readonly IBeforeSubscribe _beforeSubscribe;
		private readonly MessageBrokerServer _server;

		public MessageBrokerHub(IActionScheduler actionScheduler, IBeforeSubscribe beforeSubscribe)
		{
			_server = new MessageBrokerServer(actionScheduler);
			_actionScheduler = actionScheduler;
			_beforeSubscribe = beforeSubscribe;
		}

		public string AddSubscription(Subscription subscription)
		{
			_beforeSubscribe.Invoke(subscription);

			var route = subscription.Route();
			
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("New subscription from client {0} with route {1} (Id: {2}).", Context.ConnectionId,
								   route, MessageBrokerServer.RouteToGroupName(route));
			}


			Groups.Add(Context.ConnectionId, MessageBrokerServer.RouteToGroupName(route))
				  .ContinueWith(t => Logger.InfoFormat("Added subscription {0}.", route));

			return route;
		}

		public void RemoveSubscription(string route)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Remove subscription from client {0} with route {1} (Id: {2}).", Context.ConnectionId, route,
								   route.GetHashCode());
			}
			Groups.Remove(Context.ConnectionId, route);
		}

		public void NotifyClients(Notification notification)
		{
			_server.NotifyClients(Clients, Context.ConnectionId, notification);
		}

		public void NotifyClientsMultiple(IEnumerable<Notification> notifications)
		{
			_server.NotifyClientsMultiple(Clients, Context.ConnectionId, notifications);
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