namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public interface ISignalR
	{
		void AddConnectionToGroup(string groupName, string connectionId);
		void RemoveConnectionFromGroup(string groupName, string connectionId);
		void CallOnEventMessage(string groupName, string route, Message message);
	}
}