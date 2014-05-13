using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;
using Subscription = Microsoft.AspNet.SignalR.Client.Hubs.Subscription;

namespace Teleopti.MessagingTest.SignalR
{
	public class HubProxyFake : IHubProxyWrapper
	{
		public readonly IList<Notification> NotifyClientsInvokedWith = new List<Notification>();
		public readonly IList<Notification> NotifyClientsMultipleInvokedWith = new List<Notification>();

		public Task Invoke(string method, params object[] args)
		{
			if (method == "NotifyClients")
				NotifyClientsInvokedWith.Add(args.First() as Notification);
			if (method == "NotifyClientsMultiple")
				NotifyClientsMultipleInvokedWith.Add(args.First() as Notification);
			return TaskHelper.MakeDoneTask();
		}

		public ISubscriptionWrapper Subscribe(string eventName)
		{
			return new SubscriptionWrapper(new Subscription());
		}
	}
}