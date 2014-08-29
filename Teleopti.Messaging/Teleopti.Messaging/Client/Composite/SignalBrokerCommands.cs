using System.Collections.Generic;
using log4net;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client.Composite
{
	public class SignalBrokerCommands
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(SignalBrokerCommands));
		private readonly ISignalRClient _client;
		private const string notifyclients = "NotifyClients";
		private const string notifyclientsmultiple = "NotifyClientsMultiple";
		private const string addsubscription = "AddSubscription";
		private const string removesubscription = "RemoveSubscription";

		public SignalBrokerCommands(ISignalRClient client)
		{
			_client = client;
		}

		public void AddSubscription(Subscription subscription)
		{
			_logger.Debug("AddSubscription");
			_client.Call(addsubscription, subscription);
		}

		public void RemoveSubscription(string route)
		{
			_logger.Debug("RemoveSubscription");
			_client.Call(removesubscription, route);
		}

		public void NotifyClients(Notification notification)
		{
			_logger.Debug("NotifyClients");
			_client.Call(notifyclients, notification);
		}

		public void NotifyClients(IEnumerable<Notification> notifications)
		{
			_logger.Debug("NotifyClients");
			_client.Call(notifyclientsmultiple, notifications);
		}

	}

}