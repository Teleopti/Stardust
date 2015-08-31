﻿using System;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Client.Composite;
using Teleopti.Messaging.Client.SignalR;
using Teleopti.Messaging.Client.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR.ImplementationDetailsTests.TestDoubles
{
	public class MessageBrokerCompositeClientForTest : MessageBrokerCompositeClient
	{
		[CLSCompliant(false)]
		public static MessageBrokerCompositeClientForTest Make(
			IMessageFilterManager typeFilter,
			IHubConnectionWrapper hubConnection
			)
		{
			var signalRClient = new signalRClientForTest(hubConnection);
			var sender = new SignalRSender(signalRClient);
			return new MessageBrokerCompositeClientForTest(typeFilter, signalRClient, sender);
		}

		private MessageBrokerCompositeClientForTest(
			IMessageFilterManager typeFilter,
			ISignalRClient client,
			IMessageSender sender)
			: base(typeFilter, client, sender, null, null, null, null, client)
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