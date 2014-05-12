using log4net;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR
{
	public class LoggingSignalSenderForTest : SignalSender
	{
		private readonly IHubConnectionWrapper _hubConnection;

		public LoggingSignalSenderForTest(IHubConnectionWrapper hubConnection, ILog logger)
			: base("http://neeedsToBeSet", logger)
		{
			_hubConnection = hubConnection;
		}

		protected override IHubConnectionWrapper MakeHubConnection()
		{
			return _hubConnection;
		}
	}
}