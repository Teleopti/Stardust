using System;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Client.SignalR;
using Teleopti.Messaging.Client.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR.ImplementationDetailsTests.TestDoubles
{
	[CLSCompliant(false)]
	public class SignalRClientForTest : SignalRClient
	{
		private readonly IHubConnectionWrapper _hubConnection;
		private readonly SignalRSender _sender;

		public SignalRClientForTest(IHubConnectionWrapper hubConnection, IConnectionKeepAliveStrategy connectionKeepAliveStrategy, ITime time)
			: base("http://neeedsToBeSet", new[] { connectionKeepAliveStrategy }, time)
		{
			_hubConnection = hubConnection;
			_sender = new SignalRSender(this);
		}

		public SignalRClientForTest(IHubConnectionWrapper hubConnection)
			: base("http://neeedsToBeSet", new IConnectionKeepAliveStrategy[] { }, new Time(new Now()))
		{
			_hubConnection = hubConnection;
			_sender = new SignalRSender(this);
		}

		protected override IHubConnectionWrapper MakeHubConnection()
		{
			return _hubConnection;
		}

		public void SendNotification(Message message)
		{
			_sender.Send(message);
		}
	}
}