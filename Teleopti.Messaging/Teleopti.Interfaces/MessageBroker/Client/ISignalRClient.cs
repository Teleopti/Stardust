namespace Teleopti.Interfaces.MessageBroker.Client
{
	public interface ISignalRClient
	{
		bool IsAlive { get; }
		void StartBrokerService(bool useLongPolling = false);
		void Call(string methodName, params object[] args);
	}
}