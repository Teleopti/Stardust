namespace Teleopti.Ccc.Domain.MessageBroker
{
	public interface ISignalR
	{
		void CallOnEventMessage(string groupName, string route, Interfaces.MessageBroker.Notification notification);
	}
}