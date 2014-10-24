using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Teleopti.Ccc.Web.Broker
{
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
				return _clients ?? (_clients = new HubCallerConnectionContext(base.Clients));
			}
			set
			{
				_clients = value;
				base.Clients = _clients as HubConnectionContext;
			}
		}

		// ... Rest of hub code
	}

	public interface IHubClientContext
	{
		dynamic Others { get; set; }
		dynamic Caller { get; set; }
		dynamic Group(string groupName, params string[] excludeConnectionIds);
	}

	public class HubCallerConnectionContext : HubConnectionContext, IHubClientContext
	{
		private readonly IHubCallerConnectionContext<dynamic> _context;
		public HubCallerConnectionContext(IHubCallerConnectionContext<dynamic> context)
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

	public class CustomHubConnectionContext : HubConnectionContext, IHubClientContext
	{
		private readonly IHubConnectionContext<dynamic> _context;
		public CustomHubConnectionContext(IHubConnectionContext<dynamic> context)
		{
			All = context.All;
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