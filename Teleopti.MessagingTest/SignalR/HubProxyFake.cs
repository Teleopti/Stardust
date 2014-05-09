using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR
{
	public class HubProxyFake : IHubProxyWrapper
	{
		public readonly IList<Notification> NotifyClientsInvokedWith = new List<Notification>();

		private static Task<object> makeDoneTask()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetResult(null);
			return taskCompletionSource.Task;
		}

		public Task Invoke(string method, params object[] args)
		{
			if (method == "NotifyClients")
				NotifyClientsInvokedWith.Add(args.First() as Notification);
			return makeDoneTask();
		}

		public Task<T> Invoke<T>(string method, params object[] args)
		{
			throw new NotImplementedException();
		}

		public ISubscriptionWrapper Subscribe(string eventName)
		{
			throw new NotImplementedException();
		}

		public JToken this[string name]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public JsonSerializer JsonSerializer { get; private set; }
	}
}