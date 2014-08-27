using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Messaging.SignalR
{
	public class SignalBrokerCommands
	{
		private readonly ILog _logger;
		private readonly IStateAccessor _hubProxy;
		private const string notifyclients = "NotifyClients";
		private const string notifyclientsmultiple = "NotifyClientsMultiple";
		private const string addsubscription = "AddSubscription";
		private const string removesubscription = "RemoveSubscription";

		public SignalBrokerCommands(ILog logger, IStateAccessor hubProxy)
		{
			_logger = logger;
			_hubProxy = hubProxy;
		}

		public void AddSubscription(Subscription subscription)
		{
			_logger.Debug("AddSubscription");
			call(addsubscription, subscription);
		}

		public void RemoveSubscription(string route)
		{
			_logger.Debug("RemoveSubscription");
			call(removesubscription, route);
		}

		public void NotifyClients(Notification notification)
		{
			_logger.Debug("NotifyClients");
			call(notifyclients, notification);
		}

		public void NotifyClients(IEnumerable<Notification> notifications)
		{
			_logger.Debug("NotifyClients");
			call(notifyclientsmultiple, notifications);
		}

		private void call(string methodName, params object[] args)
		{
			_hubProxy.IfProxyConnected(p =>
			{
				var task = p.Invoke(methodName, args);

				task.ContinueWith(t =>
				{
					if (t.IsFaulted && t.Exception != null)
						_logger.Info("An error occurred on task calling " + methodName, t.Exception);
				}, TaskContinuationOptions.OnlyOnFaulted);
			});
		}
	}
}