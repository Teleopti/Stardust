﻿using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR.TestDoubles
{
	public class SignalBrokerForTest : SignalBroker
	{
		private readonly IHubConnectionWrapper _hubConnection;

		public SignalBrokerForTest(IMessageFilterManager typeFilter, IHubConnectionWrapper hubConnection)
			: base(null, typeFilter, new IConnectionKeepAliveStrategy[] { }, new Time(new Now()))
		{
			_hubConnection = hubConnection;
			ConnectionString = "http://neeedsToBeSet";
		}

		protected override IHubConnectionWrapper MakeHubConnection()
		{
			return _hubConnection;
		}

	}
}