using System;
using System.Linq;
using Microsoft.AspNet.SignalR.Client;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;
using Teleopti.MessagingTest.SignalR.TestDoubles;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class SignalConnectionRecreateOnPingLossTest
	{
		private IHubConnectionWrapper stubHubConnection(IHubProxyWrapper hubProxy)
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.State).Return(ConnectionState.Connected);
			hubConnection.Stub(x => x.Start()).Return(TaskHelper.MakeDoneTask());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			return hubConnection;
		}

		[Test]
		public void ShouldRecreateConnectionWhenNoReplyFromPingInXMinutes()
		{
			var time = new FakeTime(new DateTime(2001, 1, 1, 12, 0, 0, DateTimeKind.Utc));
			var hubProxy1 = new HubProxyFake();
			var hubProxy2 = new HubProxyFake();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var hubConnection2 = stubHubConnection(hubProxy2);
			var target = new MultiConnectionSignalSenderForTest(new[] {hubConnection1, hubConnection2}, new RecreateOnNoPingReply(TimeSpan.FromMinutes(3)), time);
			target.StartBrokerService();

			hubProxy1.BreakTheConnection();
			time.Passes(TimeSpan.FromMinutes(4));
			var notification = new Notification();
			target.SendNotification(notification);

			hubProxy2.NotifyClientsInvokedWith.Single().Should().Be(notification);
		}

		[Test]
		public void ShouldNotRecreateUntilTimeout()
		{
			var time = new FakeTime();
			var hubProxy = new HubProxyFake();
			var hubConnection = stubHubConnection(hubProxy);
			var target = new MultiConnectionSignalSenderForTest(new[] { hubConnection }, new RecreateOnNoPingReply(TimeSpan.FromMinutes(2)), time);
			target.StartBrokerService();

			hubProxy.BreakTheConnection();
			time.Passes(TimeSpan.FromSeconds(119));
			var notification = new Notification();
			target.SendNotification(notification);

			hubProxy.NotifyClientsInvokedWith.Single().Should().Be(notification);
		}

		[Test]
		public void ShouldSendNotificationsOnCurrentConnection()
		{
			var time = new FakeTime(new DateTime(2013, 1, 1, 12, 0, 0, DateTimeKind.Utc));
			var hubProxy1 = new HubProxyFake();
			var hubProxy2 = new HubProxyFake();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var hubConnection2 = stubHubConnection(hubProxy2);
			var target = new MultiConnectionSignalSenderForTest(new[] { hubConnection1, hubConnection2 }, new RecreateOnNoPingReply(TimeSpan.FromMinutes(1)), time);
			target.StartBrokerService();

			var notification1 = new Notification();
			target.SendNotification(notification1);
			hubProxy1.BreakTheConnection();
			time.Passes(TimeSpan.FromMinutes(2));
			var notification2 = new Notification();
			target.SendNotification(notification2);

			hubProxy1.NotifyClientsInvokedWith.Single().Should().Be(notification1);
			hubProxy2.NotifyClientsInvokedWith.Single().Should().Be(notification2);
		}

		[Test]
		public void ShouldCreateConnectionsInTheBackground()
		{
			var time = new FakeTime();
			var hubProxy1 = new HubProxyFake();
			var hubProxy2 = new HubProxyFake();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var hubConnection2 = stubHubConnection(hubProxy2);
			var target = new MultiConnectionSignalSenderForTest(new[] { hubConnection1, hubConnection2 }, new RecreateOnNoPingReply(TimeSpan.FromMinutes(1)), time);
			target.StartBrokerService();

			hubProxy1.BreakTheConnection();
			time.Passes(TimeSpan.FromMinutes(2));

			target.CurrentConnection.Should().Be(hubConnection2);
		}

		[Test]
		public void ShouldSubscribeToNotificationsOnFirstConnection()
		{
			var time = new FakeTime();
			var wasEventHandlerCalled = false;
			var hubProxy1 = new HubProxyFake();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var target = new MultiConnectionSignalBrokerForTest(new MessageFilterManagerFake(),
				new[] {hubConnection1}, new RecreateOnNoPingReply(TimeSpan.FromMinutes(1)), time);
			target.StartMessageBroker();

			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
				typeof (IInterfaceForTest));
			target.SendEventMessage(string.Empty, Guid.Empty, DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty,
				typeof (IInterfaceForTest), DomainUpdateType.Update, new byte[] {});

			wasEventHandlerCalled.Should().Be(true);
		}

		[Test]
		public void ShouldSubscribeToNotificationsOnRecreatedConnection()
		{
			var time = new FakeTime();
			var wasEventHandlerCalled = false;
			var hubProxy1 = new HubProxyFake();
			var hubProxy2 = new HubProxyFake();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var hubConnection2 = stubHubConnection(hubProxy2);
			var target = new MultiConnectionSignalBrokerForTest(new MessageFilterManagerFake(),
				new[] {hubConnection1, hubConnection2}, new RecreateOnNoPingReply(TimeSpan.FromMinutes(1)), time);
			target.StartMessageBroker();

			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
				typeof (IInterfaceForTest));
			hubProxy1.BreakTheConnection();
			time.Passes(TimeSpan.FromMinutes(2));
			target.SendEventMessage(string.Empty, Guid.Empty, DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty,
				typeof (IInterfaceForTest), DomainUpdateType.Update, new byte[] {});

			wasEventHandlerCalled.Should().Be(true);
		}

		[Test]
		public void ShouldStartRecreatedConnection()
		{
			var time = new FakeTime();
			var hubProxy1 = new HubProxyFake();
			var hubProxy2 = new HubProxyFake();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var hubConnection2 = stubHubConnection(hubProxy2);
			var target = new MultiConnectionSignalSenderForTest(new[] { hubConnection1, hubConnection2 }, new RecreateOnNoPingReply(TimeSpan.FromMinutes(1)), time);
			target.StartBrokerService();

			hubProxy1.BreakTheConnection();
			time.Passes(TimeSpan.FromMinutes(2));

			hubConnection2.AssertWasCalled(x => x.Start());
		}

		[Test]
		public void ShouldStopPingOnClose()
		{
			var time = new FakeTime();
			var hubProxy1 = new HubProxyFake();
			var hubProxy2 = new HubProxyFake();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var hubConnection2 = stubHubConnection(hubProxy2);
			var target = new MultiConnectionSignalSenderForTest(new[] { hubConnection1, hubConnection2 }, new RecreateOnNoPingReply(TimeSpan.FromMinutes(1)), time);
			target.StartBrokerService();
			target.Dispose();

			hubProxy1.BreakTheConnection();
			time.Passes(TimeSpan.FromMinutes(2));

			hubConnection2.AssertWasNotCalled(x => x.Start());
		}

		private interface IInterfaceForTest
		{

		}

	}
}