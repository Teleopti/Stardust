namespace Teleopti.Ccc.Domain.MessageBroker
{
	public interface ISignalR
	{
		void AddConnectionToGroup(string groupName, string route, string connectionId);
		void CallOnEventMessage(string groupName, string route, Interfaces.MessageBroker.Notification notification);
	}
}