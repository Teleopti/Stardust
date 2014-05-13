using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;
using Subscription = Microsoft.AspNet.SignalR.Client.Hubs.Subscription;

namespace Teleopti.MessagingTest.SignalR
{
	public class HubThatRepliesToPing : IHubProxyWrapper
	{
		private readonly PingReplySubscription pingReply = new PingReplySubscription();
		private bool _broken;

		public readonly IList<Notification> NotifyClientsInvokedWith = new List<Notification>();

		public ISubscriptionWrapper Subscribe(string eventName)
		{
			if (eventName == "Pong")
				return pingReply;
			return new SubscriptionWrapper(new Subscription());
		}

		public Task Invoke(string method, params object[] args)
		{
			if (method == "Ping" && !_broken)
			{
				pingReply.ReplyToPing();
			}
			if (method == "NotifyClients")
				NotifyClientsInvokedWith.Add(args.First() as Notification);
			return TaskHelper.MakeDoneTask();
		}

		public void BreakTheConnection()
		{
			_broken = true;
		}
	}

	public class PingReplySubscription : ISubscriptionWrapper
	{
		public event Action<IList<JToken>> Received;

		public void ReplyToPing()
		{
			Received(null);
		}
	}
}