using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.DomainTest.MessageBroker
{
	[CLSCompliant(false)]
	[TestFixture]
	[MessageBrokerServerTest]
	public class SignalRPublishTest
	{
		public IMessageBrokerServer Server;
		public FakeSignalR SignalR;

		[Test]
		public void ShouldPublishToSignalR()
		{
			var notification = new Interfaces.MessageBroker.Notification();
			Server.NotifyClients(notification);
			SignalR.SentMessage.Should().Be(notification);
		}

		[Test]
		public void ShouldPublishToAllRoutes()
		{
			var notification = new Interfaces.MessageBroker.Notification();

			Server.NotifyClients(notification);

			SignalR.SentToGroups.Should().Have.SameValuesAs(notification.Routes().Select(MessageBrokerServer.RouteToGroupName));
			SignalR.SentRoutes.Should().Have.SameValuesAs(notification.Routes());
		}

		[Test]
		public void ShouldPublishMultipleNotifications()
		{
			var notifications = new[] { new Interfaces.MessageBroker.Notification(), new Interfaces.MessageBroker.Notification() };
			Server.NotifyClientsMultiple(notifications);
			SignalR.SentMessages.Should().Have.SameValuesAs(notifications);
		}
	}

	public class FakeSignalR : ISignalR
	{
		public Interfaces.MessageBroker.Notification SentMessage;
		public string SentToGroup;
		public string SentRoute;

		public IList<string> SentToGroups = new List<string>();
		public IList<string> SentRoutes = new List<string>();
		public IList<Interfaces.MessageBroker.Notification> SentMessages = new List<Interfaces.MessageBroker.Notification>();

		public void CallOnEventMessage(string groupName, string route, Interfaces.MessageBroker.Notification notification)
		{
			SentToGroup = groupName;
			SentRoute = route;
			SentMessage = notification;

			SentToGroups.Add(groupName);
			SentRoutes.Add(route);
			SentMessages.Add(notification);
		}
	}
}
