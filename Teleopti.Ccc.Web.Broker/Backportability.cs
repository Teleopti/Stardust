using System;
using log4net;
using Microsoft.AspNet.SignalR;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[CLSCompliant(false)]
	public class BackportableHub : Hub
	{

	}

	[CLSCompliant(false)]
	public class SignalR : ISignalR
	{
		public ILog Logger = LogManager.GetLogger(typeof(MessageBrokerServer));

		public void AddConnectionToGroup(string groupName, string route, string connectionId)
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<MessageBrokerHub>();
			context.Groups.Add(connectionId, groupName)
				  .ContinueWith(t => Logger.InfoFormat("Added subscription {0}.", route));
		}

		public void CallOnEventMessage(string groupName, string route, Notification notification)
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<MessageBrokerHub>();
			context.Clients.Group(groupName).onEventMessage(notification, route);
		}

	}
}