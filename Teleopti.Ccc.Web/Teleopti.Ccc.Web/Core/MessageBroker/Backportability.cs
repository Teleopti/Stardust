using System;
using log4net;
using Microsoft.AspNet.SignalR;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Ccc.Web.Broker
{
	public class BackportableHub : Hub
	{

	}

	public class SignalR : ISignalR
	{
		public ILog Logger = LogManager.GetLogger(typeof(MessageBrokerServer));

		public void AddConnectionToGroup(string groupName, string connectionId)
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<MessageBrokerHub>();
			context.Groups.Add(connectionId, groupName);
		}

		public void RemoveConnectionFromGroup(string groupName, string connectionId)
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<MessageBrokerHub>();
			context.Groups.Remove(connectionId, groupName);
		}

		public void CallOnEventMessage(string groupName, string route, Message message)
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<MessageBrokerHub>();
			context.Clients.Group(groupName).onEventMessage(message, route);
		}

	}
}