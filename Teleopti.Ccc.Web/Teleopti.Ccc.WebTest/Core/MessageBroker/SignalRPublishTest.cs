using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker
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
			var notification = new Notification();
			Server.NotifyClients(notification);
			SignalR.SentMessage.Should().Be(notification);
		}

		[Test]
		public void ShouldPublishToAllRoutes()
		{
			var notification = new Notification();

			Server.NotifyClients(notification);

			SignalR.SentToGroups.Should().Have.SameValuesAs(notification.Routes().Select(MessageBrokerServer.RouteToGroupName));
			SignalR.SentRoutes.Should().Have.SameValuesAs(notification.Routes());
		}

		[Test]
		public void ShouldPublishMultipleNotifications()
		{
			var notifications = new[] { new Notification(), new Notification() };
			Server.NotifyClientsMultiple(notifications);
			SignalR.SentMessages.Should().Have.SameValuesAs(notifications);
		}
	}

	public class FakeSignalR : ISignalR
	{
		public Notification SentMessage;
		public string SentToGroup;
		public string SentRoute;

		public IList<string> SentToGroups = new List<string>();
		public IList<string> SentRoutes = new List<string>();
		public IList<Notification> SentMessages = new List<Notification>();

		public void CallOnEventMessage(string groupName, string route, Notification notification)
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
