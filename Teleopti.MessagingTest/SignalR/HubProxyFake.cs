using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;
using Subscription = Microsoft.AspNet.SignalR.Client.Hubs.Subscription;

namespace Teleopti.MessagingTest.SignalR
{
	public class HubProxyFake : IHubProxyWrapper
	{
		private readonly SubscriptionFake pingReplySubscription = new SubscriptionFake();
		private readonly SubscriptionFake notificationSubscription = new SubscriptionFake();
		private bool _connectionBroken;

		public readonly IList<Notification> NotifyClientsInvokedWith = new List<Notification>();
		public readonly IList<Notification> NotifyClientsMultipleInvokedWith = new List<Notification>();

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
				var notification = (Notification) args.First();
				NotifyClientsInvokedWith.Add(notification);
				RaiseNotificationSubscription(notification);
			}

			if (method == "NotifyClientsMultiple")
			{
				var notifications = (IEnumerable<Notification>) args.First();
				notifications.ForEach(notification =>
				{
					NotifyClientsMultipleInvokedWith.Add(notification);
					RaiseNotificationSubscription(notification);
				});
			}

			return TaskHelper.MakeDoneTask();
		}

		private void RaiseNotificationSubscription(Notification notification)
		{
			if (_connectionBroken) return;
			var token = JObject.Parse(JsonConvert.SerializeObject(notification));
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