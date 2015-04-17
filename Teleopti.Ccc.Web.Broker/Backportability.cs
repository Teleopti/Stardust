using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[CLSCompliant(false)]
	public class BackportableHub : TestableHub
	{
		
	}

	[CLSCompliant(false)]
	public class SignalR : ISignalR
	{
		private readonly IHubConnectionContext _connectionContext;

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

	//from https://github.com/SignalR/SignalR/pull/1127
	// this is just so that Clients is an interface that can be stubbed out
	// this will make this class obsolete: 
	// https://github.com/SignalR/SignalR/issues/1352

	public class TestableHub : Hub
	{
		private IHubClientContext _clients;
		public new IHubClientContext Clients
		{
			get
			{
				return _clients ?? (_clients = new HubClientContext(base.Clients));
			}
			set
			{
				_clients = value;
				base.Clients = _clients as HubConnectionContext;
			}
		}

		// ... Rest of hub code
	}

	public interface IHubClientContext : IHubConnectionContext
	{
		dynamic Others { get; set; }
		dynamic Caller { get; set; }
		new dynamic Group(string groupName, params string[] excludeConnectionIds);
	}

	public class HubClientContext : HubConnectionContext, IHubClientContext
	{
		private readonly HubConnectionContext _context;
		public HubClientContext(HubConnectionContext context)
		{
			All = context.All;
			Caller = context.Caller;
			Others = context.Others;
			_context = context;
		}

		public new dynamic Client(string connectionId)
		{
			return _context.Client(connectionId);
		}

		public new dynamic Group(string groupName, params string[] excludeConnectionIds)
		{
			return _context.Group(groupName, excludeConnectionIds);
		}

		//// Override other methods, as needed
	}

}