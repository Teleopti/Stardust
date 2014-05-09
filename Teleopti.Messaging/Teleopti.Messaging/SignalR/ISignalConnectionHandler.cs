using System.Threading.Tasks;

namespace Teleopti.Messaging.SignalR
{
	public interface ISignalConnectionHandler : ICallHubProxy
	{
		void StartConnection();
		void CloseConnection();
		bool IsInitialized();
	}
}