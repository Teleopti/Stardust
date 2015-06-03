using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	[TestFixture, Category("LongRunning")]
	[MessageBrokerTest]
	public class MessageBrokerServerTest
	{
		public IMessageBrokerServer Broker;

		[Test]
		public void ShouldPublishToMailbox()
		{
			var mailboxId = Guid.NewGuid().ToString();
			var businessUnitId = Guid.NewGuid().ToString();
			Broker.AddMailbox(new Subscription
			{
				BusinessUnitId = businessUnitId,
				MailboxId = mailboxId
			});

			Broker.NotifyClients(new Message
			{
				BusinessUnitId = businessUnitId
			});

			var messages = Broker.PopMessages(mailboxId);
			messages.Should().Have.Count.EqualTo(1);
		}
	}
}