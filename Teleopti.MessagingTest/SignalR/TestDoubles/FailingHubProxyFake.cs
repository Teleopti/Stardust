using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Teleopti.Messaging.Client.SignalR;
using Teleopti.Messaging.Client.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR.TestDoubles
{
	public class FailingHubProxyFake : IHubProxyWrapper
	{
		private readonly Exception _exception;

		public FailingHubProxyFake(Exception exception)
		{
			_exception = exception;
		}

		public Task Invoke(string method, params object[] args)
		{
			if (method == "NotifyClients")
				return TaskHelper.MakeFailedTask(_exception);
			return TaskHelper.MakeDoneTask();
		}

		public ISubscriptionWrapper Subscribe(string eventName)
		{
			return new SubscriptionWrapper(new Subscription());
		}
	}
}