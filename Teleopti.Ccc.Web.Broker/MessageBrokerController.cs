using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerController : Controller
	{
		public Func<IHubContext> HubContext = () => GlobalHost.ConnectionManager.GetHubContext<MessageBrokerHub>();

		private readonly MessageBrokerServer _server;

		public MessageBrokerController(IActionScheduler actionScheduler)
		{
			_server = new MessageBrokerServer(actionScheduler);
		}

		public void NotifyClients(Notification notification)
		{
			_server.NotifyClients(new CustomHubConnectionContext<dynamic>(HubContext().Clients), "POST", notification);
		}

		public void NotifyClientsMultiple(IEnumerable<Notification> notifications)
		{
			_server.NotifyClientsMultiple(new CustomHubConnectionContext<dynamic>(HubContext().Clients), "POST", notifications);
		}
	}

	public class CustomHubConnectionContext<T> : IHubCallerConnectionContext<T>
	{
		private readonly IHubConnectionContext<object> _clients;

		public CustomHubConnectionContext(IHubConnectionContext<object> clients)
		{
			_clients = clients;
		}

		public T AllExcept(params string[] excludeConnectionIds)
		{
			return (dynamic)_clients.AllExcept(excludeConnectionIds);
		}

		public T Client(string connectionId)
		{
			return (dynamic)_clients.Client(connectionId);
		}

		public T Clients(IList<string> connectionIds)
		{
			return (dynamic)_clients.Clients(connectionIds);
		}

		public T Group(string groupName, params string[] excludeConnectionIds)
		{
			return (dynamic) _clients.Group(groupName, excludeConnectionIds);
		}

		public T Groups(IList<string> groupNames, params string[] excludeConnectionIds)
		{
			return (dynamic)_clients.Groups(groupNames, excludeConnectionIds);
		}

		public T User(string userId)
		{
			return (dynamic) _clients.User(userId);
		}

		public T Users(IList<string> userIds)
		{
			return (dynamic)_clients.Users(userIds);
		}

		public T All
		{
			get { return (dynamic) _clients.All; }
		}

		public T OthersInGroup(string groupName)
		{
			return Group(groupName);
		}

		public T OthersInGroups(IList<string> groupNames)
		{
			return Groups(groupNames);
		}

		public T Caller { get; private set; }
		public dynamic CallerState { get; private set; }
		public T Others { get { return All; } }
	}
}