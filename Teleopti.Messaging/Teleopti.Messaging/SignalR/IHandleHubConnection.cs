namespace Teleopti.Messaging.SignalR
{
	public interface IHandleHubConnection
	{
		void StartConnection();
		void CloseConnection();
		bool IsConnected();
	}
}