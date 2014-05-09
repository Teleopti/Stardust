using log4net;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR
{
	public class SignalSenderForTest : SignalSender
	{
		private readonly IHubConnectionWrapper _hubConnection;

		public SignalSenderForTest(IHubConnectionWrapper hubConnection, ILog logger)
			: base("http://neeedsToBeSet")
		{
			_hubConnection = hubConnection;
			Logger = logger;
		}

		protected override ILog MakeLogger()
		{
			return Logger ?? base.MakeLogger();
		}

		protected override IHubConnectionWrapper MakeHubConnection()
		{
			return _hubConnection;
		}
	}
}