using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	[TestFixture]
	[AnalyticsDatabaseTest]
	public class MessageBrokerServerTest
	{
		public IMessageBrokerServer Broker;
		public FakeTime Time;

		[Test]
		public void ShouldPublishToMailbox()
		{
			var mailboxId = Guid.NewGuid().ToString();
			var businessUnitId = Guid.NewGuid().ToString();
			var subscription = new Subscription
			{
				BusinessUnitId = businessUnitId,
			};
			Broker.PopMessages(subscription.Route(), mailboxId);

			Broker.NotifyClients(new Message
			{
				BusinessUnitId = businessUnitId
			});
			Time.Passes("1".Seconds());

			var messages = Broker.PopMessages(subscription.Route(), mailboxId);
			messages.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishMultipleMessagesToMailbox()
		{
			var mailboxId = Guid.NewGuid().ToString();
			var businessUnitId = Guid.NewGuid().ToString();
			var subscription = new Subscription
			{
				BusinessUnitId = businessUnitId,
				MailboxId = mailboxId
			};
			Broker.PopMessages(subscription.Route(), mailboxId);

			Broker.NotifyClients(new Message
			{
				BusinessUnitId = businessUnitId
			});
			Broker.NotifyClients(new Message
			{
				BusinessUnitId = businessUnitId
			});
			Time.Passes("1".Seconds());

			var messages = Broker.PopMessages(subscription.Route(), mailboxId);
			messages.Should().Have.Count.EqualTo(2);
		}
	}
}