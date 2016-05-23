using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNet.SignalR.Client;

namespace Teleopti.Messaging.Client.SignalR.Wrappers
{
	public class HubProxyWrapper : IHubProxyWrapper
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(HubProxyWrapper));
		private readonly IHubProxy _hubProxy;

		public HubProxyWrapper(IHubProxy hubProxy)
		{
			_hubProxy = hubProxy;
		}

		public Task Invoke(string method, params object[] args)
		{
			try
			{
				return _hubProxy.Invoke(method, args);
			}
			catch (Exception ex)
			{
				var arguments = (args != null && args.Any()) ? string.Join("|", args) : string.Empty;
				var errorMessage = $"Failed to invoke method \"{method}\" " + (string.IsNullOrEmpty(arguments)
					? "without argument."
					: $"with arguments \"{arguments}\".");
				logger.Error(errorMessage, ex);
				throw;
			}
		}

		public ISubscriptionWrapper Subscribe(string eventName)
		{
			return new SubscriptionWrapper(_hubProxy.Subscribe(eventName));
		}
	}
}