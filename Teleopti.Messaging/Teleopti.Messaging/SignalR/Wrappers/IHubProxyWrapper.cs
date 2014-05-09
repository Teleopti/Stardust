using System.Threading.Tasks;

namespace Teleopti.Messaging.SignalR.Wrappers
{
	public interface IHubProxyWrapper
	{
		Task Invoke(string method, params object[] args);
		ISubscriptionWrapper Subscribe(string eventName);
	}
}