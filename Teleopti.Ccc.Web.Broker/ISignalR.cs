using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	public interface ISignalR
	{
		void CallOnEventMessage(string groupName, string route, Notification notification);
	}
}