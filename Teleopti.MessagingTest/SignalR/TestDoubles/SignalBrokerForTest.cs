using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR.TestDoubles
{
	public class SignalBrokerForTest : SignalBroker
	{
		public SignalBrokerForTest(IMessageFilterManager typeFilter, IHubConnectionWrapper hubConnection)
			: base(typeFilter, new signalRClientForTest(hubConnection))
		{
		}

		private class signalRClientForTest : SignalRClient
		{
			private readonly IHubConnectionWrapper _hubConnection;

			public signalRClientForTest(IHubConnectionWrapper hubConnection)
				: base("http://neeedsToBeSet", new IConnectionKeepAliveStrategy[] { }, new Time(new Now()))
			{
				_hubConnection = hubConnection;
			}

			protected override IHubConnectionWrapper MakeHubConnection()
			{
				return _hubConnection;
			}
		}

	}
}