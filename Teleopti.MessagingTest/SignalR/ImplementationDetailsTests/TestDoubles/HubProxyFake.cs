using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Messaging.Client.SignalR;
using Teleopti.Messaging.Client.SignalR.Wrappers;
using Subscription = Microsoft.AspNet.SignalR.Client.Hubs.Subscription;

namespace Teleopti.MessagingTest.SignalR.ImplementationDetailsTests.TestDoubles
{
	public class HubProxyFake : IHubProxyWrapper
	{
		private readonly SubscriptionFake pingReplySubscription = new SubscriptionFake();
		private readonly SubscriptionFake notificationSubscription = new SubscriptionFake();
		private string _subscriptedToRoute;
		private bool _connectionBroken;

		public readonly IList<Message> NotifyClientsInvokedWith = new List<Message>();
		public readonly IList<Message> NotifyClientsMultipleInvokedWith = new List<Message>();

		public ISubscriptionWrapper Subscribe(string eventName)
		{
			if (eventName == "Pong")
				return pingReplySubscription;
			if (eventName == "OnEventMessage")
				return notificationSubscription;
			return new SubscriptionWrapper(new Subscription());
		}

		public Task Invoke(string method, params object[] args)
		{
			if (method == "Ping" && !_connectionBroken)
				pingReplySubscription.RaiseRecieved(null);

			if (method == "NotifyClients")
			{
				var notification = (Message) args.First();
				NotifyClientsInvokedWith.Add(notification);
				raiseNotificationSubscription(notification);
			}

			if (method == "NotifyClientsMultiple")
			{
				var notifications = (IEnumerable<Message>) args.First();
				notifications.ForEach(notification =>
				{
					NotifyClientsMultipleInvokedWith.Add(notification);
					raiseNotificationSubscription(notification);
				});
			}

			if (method == "AddSubscription")
			{
				var subscription = (Ccc.Domain.MessageBroker.Subscription) args.First();
				_subscriptedToRoute = subscription.Route();
			}

			return TaskHelper.MakeDoneTask();
		}

		private void raiseNotificationSubscription(Message message)
		{
			if (_connectionBroken)
				return;
			if (_subscriptedToRoute == null)
				return;
			if (!message.Routes().Contains(_subscriptedToRoute))
				return;
			var token = JObject.Parse(JsonConvert.SerializeObject(message));
			notificationSubscription.RaiseRecieved(new List<JToken>(new JToken[] {token}));
		}

		public void BreakTheConnection()
		{
			_connectionBroken = true;
		}
	}

	public class SubscriptionFake : ISubscriptionWrapper
	{
		public event Action<IList<JToken>> Received;

		public void RaiseRecieved(IList<JToken> obj)
		{
			if (Received != null)
				Received(obj);
		}
	}
}