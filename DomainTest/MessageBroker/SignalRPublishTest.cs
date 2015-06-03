﻿using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.MessageBroker
{
	[TestFixture]
	[MessageBrokerServerTest]
	public class SignalRPublishTest
	{
		public IMessageBrokerServer Server;
		public FakeSignalR SignalR;

		[Test]
		public void ShouldPublishToSignalR()
		{
			var notification = new Interfaces.MessageBroker.Message();
			Server.NotifyClients(notification);
			SignalR.SentMessage.Should().Be(notification);
		}

		[Test]
		public void ShouldPublishToAllRoutes()
		{
			var notification = new Interfaces.MessageBroker.Message();

			Server.NotifyClients(notification);

			SignalR.SentToGroups.Should().Have.SameValuesAs(notification.Routes().Select(MessageBrokerServer.RouteToGroupName));
			SignalR.SentRoutes.Should().Have.SameValuesAs(notification.Routes());
		}

		[Test]
		public void ShouldPublishMultipleNotifications()
		{
			var notifications = new[] { new Interfaces.MessageBroker.Message(), new Interfaces.MessageBroker.Message() };
			Server.NotifyClientsMultiple(notifications);
			SignalR.SentMessages.Should().Have.SameValuesAs(notifications);
		}
	}
}
