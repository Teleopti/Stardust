using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker.LegacyTests
{
	[TestFixture]
	[Ignore]
	public class MessageBrokerHubTest
	{
		[Test]
		public void ShouldSubscribe()
		{
			var target = new MessageBrokerHub(new ActionImmediate(), new MessageBrokerServer(new ActionImmediate(), new SignalR()), new SubscriptionPassThrough());
			var hubBuilder = new TestHubBuilder();
			var subscription = new Subscription {DataSource = "something"};
			var notification = new Notification {DataSource = "something"};
			var notified = false;
			var client = hubBuilder.FakeClient(
				"onEventMessage",
				new Action<Notification, string>((n, r) =>
					{
						if (r != subscription.Route()) return;
						notified = true;
						n.Should().Be(notification);
					}));
			hubBuilder.SetupHub(target, client);

			target.AddSubscription(subscription);

			target.NotifyClientsMultiple(new[] {notification});

			notified.Should().Be.True();
		}

		[Test]
		public void ShouldUnsubscribe()
		{
			var target = new MessageBrokerHub(new ActionImmediate(), new MessageBrokerServer(new ActionImmediate(), new SignalR()), new SubscriptionPassThrough());
			var hubBuilder = new TestHubBuilder();
			var subscription = new Subscription { DataSource = "something" };
			hubBuilder.SetupHub(target);

			target.RemoveSubscription(subscription.Route());

			target.Groups.AssertWasCalled(x => x.Remove(target.Context.ConnectionId, subscription.Route()));

		}

		[Test]
		public void ShouldPongOnPing()
		{
			var target = new MessageBrokerHub(new ActionImmediate(), new MessageBrokerServer(new ActionImmediate(), new SignalR()), new SubscriptionPassThrough());
			var hubBuilder = new TestHubBuilder();
			var _ponged = false;
			var client = hubBuilder.FakeClient(
				"Pong",
				() =>
				{
					_ponged = true;
				});
			hubBuilder.SetupHub(target, client);

			target.Ping();

			_ponged.Should().Be.True();
		}

		[Test]
		public void Ping_WithAnIdentification_ShouldPongWithsameIdentification()
		{
			var target = new MessageBrokerHub(new ActionImmediate(), new MessageBrokerServer(new ActionImmediate(), new SignalR()), new SubscriptionPassThrough());
			var hubBuilder = new TestHubBuilder();
			var pongedWith = 0d;
			var client = hubBuilder.FakeClient<double>(
				"Pong",
				d =>
				{
					pongedWith = d;
				});
			hubBuilder.SetupHub(target, client);

			target.PingWithId(12);

			pongedWith.Should().Be(12);
		}

		[Test]
		public void Ping_WithNumberOfMessages_ShouldSendThatNumberOfMessageswithSignalR()
		{
			var expectedNumberOfSentMessages = 17;
			var target = new MessageBrokerHub(new ActionImmediate(), new MessageBrokerServer(new ActionImmediate(), new SignalR()), new SubscriptionPassThrough());
			var hubBuilder = new TestHubBuilder();

			var numberOfpongs = 0;
			var client = hubBuilder.FakeClient(
				"Pong",
				() =>
				{
					numberOfpongs++;
				});
			hubBuilder.SetupHub(target, client);

			

			target.Ping(expectedNumberOfSentMessages);

			Assert.That(numberOfpongs, Is.EqualTo(expectedNumberOfSentMessages));

		}

	}
}
