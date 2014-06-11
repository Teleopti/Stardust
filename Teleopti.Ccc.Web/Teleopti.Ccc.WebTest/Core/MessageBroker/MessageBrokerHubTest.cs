using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs;
using Teleopti.Interfaces.MessageBroker;
using log4net;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker
{
	[TestFixture]
	public class MessageBrokerHubTest
	{
		public void ShouldHaveCoverageForLogging()
		{
			var target = new MessageBrokerHub(new ActionImmediate(), new SubscriptionPassThrough());
			var hubBuilder = new TestHubBuilder();
			var client = hubBuilder.FakeClient("onEventMessage", new Action<Notification, string>((n, r) => { }));
			hubBuilder.SetupHub(target, client);
			target.Logger = MockRepository.GenerateMock<ILog>();
			target.Logger.Stub(x => x.IsDebugEnabled).Return(true);

			target.AddSubscription(new Subscription());
			target.RemoveSubscription("something");
			target.NotifyClients(new Notification());
		}

		public void ShouldSubscribe()
		{
			var target = new MessageBrokerHub(new ActionImmediate(), new SubscriptionPassThrough());
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
			var target = new MessageBrokerHub(new ActionImmediate(), new SubscriptionPassThrough());
			var hubBuilder = new TestHubBuilder();
			var subscription = new Subscription { DataSource = "something" };
			hubBuilder.SetupHub(target);

			target.RemoveSubscription(subscription.Route());

			target.Groups.AssertWasCalled(x => x.Remove(target.Context.ConnectionId, subscription.Route()));

		}

		[Test]
		public void ShouldPongOnPing()
		{
			var target = new MessageBrokerHub(new ActionImmediate(), new SubscriptionPassThrough());
			var hubBuilder = new TestHubBuilder();
			var _ponged = false;
			var client = hubBuilder.FakeClient(
				"Pong",
				new Action(() =>
				{
					_ponged = true;
				}));
			hubBuilder.SetupHub(target, client);

			target.Ping();

			_ponged.Should().Be.True();
		}

		[Test]
		public void Ping_WithAnIdentification_ShouldPongWithsameIdentification()
		{
			var target = new MessageBrokerHub(new ActionImmediate(), new SubscriptionPassThrough());
			var hubBuilder = new TestHubBuilder();
			var pongedWith = 0d;
			var client = hubBuilder.FakeClient<double>(
				"Pong",
				new Action<double>((d) =>
				{
					pongedWith = d;
				}));
			hubBuilder.SetupHub(target, client);

			target.PingWithId(13);

			pongedWith.Should().Be(12);
		}


	}
}
