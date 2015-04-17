using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[CLSCompliant(false)]
	public class BackportableHub : Hub
	{

	}

	public class SignalR : ISignalR
	{
		private readonly IHubConnectionContext<dynamic> _connectionContext;

		public SignalR(IHubContext context)
		{
			_connectionContext = context.Clients;
		}

		public SignalR(IHub context)
		{
			_connectionContext = context.Clients;
		}

		public void CallOnEventMessage(string groupName, string route, Notification notification)
		{
			_connectionContext.Group(groupName).onEventMessage(notification, route);
		}
	}
}