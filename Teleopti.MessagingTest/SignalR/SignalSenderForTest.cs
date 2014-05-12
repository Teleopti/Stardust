using System;
using System.Collections;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR
{
	public class SignalSenderForTest : SignalSender
	{
		private readonly IHubConnectionWrapper _hubConnection;

		public SignalSenderForTest(IHubConnectionWrapper hubConnection)
			: base("http://neeedsToBeSet", null, new Now())
		{
			_hubConnection = hubConnection;
		}

		protected override IHubConnectionWrapper MakeHubConnection()
		{
			return _hubConnection;
		}
	}
}