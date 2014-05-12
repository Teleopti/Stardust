using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Messaging.SignalR
{
	public class SignalBrokerCommands : ISignalBrokerCommands
	{
		private readonly ILog _logger;
		private readonly ICallHubProxy _hubProxy;
		private const string notifyclients = "NotifyClients";
		private const string notifyclientsmultiple = "NotifyClientsMultiple";
		private const string addsubscription = "AddSubscription";
		private const string removesubscription = "RemoveSubscription";

		public SignalBrokerCommands(ILog logger, ICallHubProxy hubProxy)
		{
			_logger = logger;
			_hubProxy = hubProxy;
		}

		public void AddSubscription(Subscription subscription)
		{
			call(addsubscription, subscription);
		}

		public void RemoveSubscription(string route)
		{
			call(removesubscription, route);
		}

		public void NotifyClients(Notification notification)
		{
			call(notifyclients, notification);
		}

		public void NotifyClients(IEnumerable<Notification> notifications)
		{
			call(notifyclientsmultiple, notifications);
		}

		private void call(string methodName, params object[] notifications)
		{
			_hubProxy.IfProxyConnected(p =>
			{
				var task = p.Invoke(methodName, notifications);

				task.ContinueWith(t =>
				{
					if (t.IsFaulted && t.Exception != null)
						_logger.Debug("An error happened on notification task", t.Exception);
				}, TaskContinuationOptions.OnlyOnFaulted);
			});
		}
	}
}