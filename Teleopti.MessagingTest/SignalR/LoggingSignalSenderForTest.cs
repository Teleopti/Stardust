using System;
using log4net;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR
{
	public class LoggingSignalSenderForTest : SignalSender
	{
		private readonly IHubConnectionWrapper _hubConnection;

		public LoggingSignalSenderForTest(IHubConnectionWrapper hubConnection, ILog logger)
			: base("http://neeedsToBeSet", logger, new Ping(TimeSpan.FromMinutes(2)), new Now())
		{
			_hubConnection = hubConnection;
		}

		protected override IHubConnectionWrapper MakeHubConnection()
		{
			return _hubConnection;
		}
	}
}