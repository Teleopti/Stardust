using System.Threading.Tasks;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR
{
	public class HubProxySubscribableFake : IHubProxyWrapper
	{
		private readonly ISubscriptionWrapper _subscription;

		public HubProxySubscribableFake(ISubscriptionWrapper subscription)
		{
			_subscription = subscription;
		}

		public Task Invoke(string method, params object[] args)
		{
			return TaskHelper.MakeDoneTask();
		}

		public ISubscriptionWrapper Subscribe(string eventName)
		{
			return _subscription;
		}
	}
}