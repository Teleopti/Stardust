using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR
{
	public class HubProxyFake : IHubProxyWrapper
	{
		public readonly IList<Notification> NotifyClientsInvokedWith = new List<Notification>();

		public Task Invoke(string method, params object[] args)
		{
			if (method == "NotifyClients")
				NotifyClientsInvokedWith.Add(args.First() as Notification);
			return TaskHelper.MakeDoneTask();
		}

		public ISubscriptionWrapper Subscribe(string eventName)
		{
			throw new NotImplementedException();
		}
	}
}