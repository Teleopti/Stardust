using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;
using Subscription = Microsoft.AspNet.SignalR.Client.Hubs.Subscription;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class SignalConnectionPingTest
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
			var hubProxy1 = new HubThatRepliesToPing();
			var hubProxy2 = new HubThatRepliesToPing();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var hubConnection2 = stubHubConnection(hubProxy2);
			var target = new MultiConnectionSignalSenderForTest(new[] {hubConnection1, hubConnection2}, new Ping(TimeSpan.FromMinutes(3)), time);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

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
			var hubProxy = new HubThatRepliesToPing();
			var hubConnection = stubHubConnection(hubProxy);
			var target = new MultiConnectionSignalSenderForTest(new[] { hubConnection }, new Ping(TimeSpan.FromMinutes(2)), time);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

			hubProxy.BreakTheConnection();
			time.Passes(TimeSpan.FromSeconds(119));
			var notification = new Notification();
			target.SendNotification(notification);

			hubProxy.NotifyClientsInvokedWith.Single().Should().Be(notification);
		}

		[Test]
		public void ShouldSendNotificationsOnCurrentConnection()
		{
			var time = new FakeTime();
			var hubProxy1 = new HubThatRepliesToPing();
			var hubProxy2 = new HubThatRepliesToPing();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var hubConnection2 = stubHubConnection(hubProxy2);
			var target = new MultiConnectionSignalSenderForTest(new[] { hubConnection1, hubConnection2 }, new Ping(TimeSpan.FromMinutes(1)), time);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

			var notification1 = new Notification();
			target.SendNotification(notification1);
			hubProxy1.BreakTheConnection();
			time.Passes(TimeSpan.FromMinutes(2));
			var notification2 = new Notification();
			target.SendNotification(notification2);

			hubProxy1.NotifyClientsInvokedWith.Single().Should().Be(notification1);
			hubProxy2.NotifyClientsInvokedWith.Single().Should().Be(notification2);
		}

		[Test, Ignore]
		public void ShouldCreateConnectionsInTheBackground()
		{
			var time = new FakeTime();
			var hubProxy1 = new HubThatRepliesToPing();
			var hubProxy2 = new HubThatRepliesToPing();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var hubConnection2 = stubHubConnection(hubProxy2);
			var target = new MultiConnectionSignalSenderForTest(new[] { hubConnection1, hubConnection2 }, new Ping(TimeSpan.FromMinutes(1)), time);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

			hubProxy1.BreakTheConnection();
			time.Passes(TimeSpan.FromMinutes(2));

			target.CurrentConnection.Should().Be(hubConnection2);
		}


	}


	

	public class HubThatRepliesToPing : IHubProxyWrapper
	{
		private readonly PingReplySubscription pingReply = new PingReplySubscription();
		private bool _broken = false;

		public readonly IList<Notification> NotifyClientsInvokedWith = new List<Notification>();

		public ISubscriptionWrapper Subscribe(string eventName)
		{
			if (eventName == "Pong")
				return pingReply;
			return new SubscriptionWrapper(new Subscription());
		}

		public Task Invoke(string method, params object[] args)
		{
			if (method == "Ping" && !_broken)
			{
				pingReply.ReplyToPing();
			//	var task = TaskHelper.Delay(TimeSpan.FromMilliseconds(100));
			//	task.ContinueWith(t => pingReply.ReplyToPing());
			}
			if (method == "NotifyClients")
				NotifyClientsInvokedWith.Add(args.First() as Notification);
			return TaskHelper.MakeDoneTask();
		}

		public void BreakTheConnection()
		{
			_broken = true;
		}
	}

	public class PingReplySubscription : ISubscriptionWrapper
	{
		public event Action<IList<JToken>> Received;

		public void ReplyToPing()
		{
			Received(null);
		}
	}


}