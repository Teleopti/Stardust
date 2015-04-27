using System;
using Microsoft.AspNet.SignalR;
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
		public void CallOnEventMessage(string groupName, string route, Notification notification)
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<MessageBrokerHub>();
			context.Clients.Group(groupName).onEventMessage(notification, route);
		}

	}
}