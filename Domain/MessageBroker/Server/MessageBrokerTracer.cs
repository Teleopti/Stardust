using System.Collections.Concurrent;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public class MessageBrokerTracer
	{
		public class SubscriptionTrace
		{
			public string Route;
			public string ConnectionId;
		}

		private readonly ILog _log = LogManager.GetLogger(typeof(MessageBrokerTracer));
		private readonly ConcurrentBag<SubscriptionTrace> _subscriptions = new ConcurrentBag<SubscriptionTrace>();

		public void SubscriptionAdded(Subscription subscription, string connectionId)
		{
			if (!_log.IsDebugEnabled)
				return;
			var route = subscription.Route();
			if (_subscriptions.Count >= 1000)
			{
				_log.DebugFormat("Connection {0} subscribed to {1}, but im already keeping too much state...", connectionId, route);
				return;
			}
			_subscriptions.Add(new SubscriptionTrace
			{
				Route = route,
				ConnectionId = connectionId
			});
			_log.DebugFormat("Connection {0} subscribed to {1}", connectionId, route);
		}

		public void ClientsNotified(Message message)
		{
			if (!_log.IsDebugEnabled)
				return;
			var m = from t in _subscriptions
					from r in message.Routes()
					where t.Route == r
					select t;
			m.ForEach(t =>
			{
				_log.DebugFormat("Connection {0} probably received a message for {1}", t.ConnectionId, t.Route);
			});
		}
	}
}