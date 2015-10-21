using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.DomainTest.MessageBroker
{
	[TestFixture]
	[MessageBrokerServerTest]
	public class MailboxTest : ISetup
	{
		public IMessageBrokerServer Server;
		public FakeMailboxRepository Mailboxes;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeConfigReader("MessageBrokerMailboxExpirationInSeconds", "1800")).For<IConfigReader>();
		}

		[Test]
		public void ShouldAddMailbox()
		{
			var subscription = new Subscription {MailboxId = Guid.NewGuid().ToString()};

			Server.AddMailbox(subscription);

			Mailboxes.PersistedLast.Route.Should().Be(subscription.Route());
		}

		[Test]
		public void ShouldAddMailboxWithId()
		{
			var id = Guid.NewGuid();
			var subscription = new Subscription {MailboxId = id.ToString()};

			Server.AddMailbox(subscription);

			Mailboxes.PersistedLast.Id.Should().Be(id);
		}

		[Test]
		public void ShouldPublishToMailbox()
		{
			var mailbox = new Subscription
			{
				MailboxId = Guid.NewGuid().ToString(),
				BusinessUnitId = Guid.NewGuid().ToString()
			};
			Server.AddMailbox(mailbox);
			var notification = new Message
			{
				BusinessUnitId = mailbox.BusinessUnitId
			};

			Server.NotifyClients(notification);

			Mailboxes.Data.Single().PopAllMessages().Single().Routes().Should().Have.SameValuesAs(notification.Routes());
		}

		[Test]
		public void ShouldPublishToMailboxMatchingRoute()
		{
			var mailbox1Id = Guid.NewGuid();
			var mailbox2Id = Guid.NewGuid();
			var mailbox1 = new Subscription
			{
				MailboxId = mailbox1Id.ToString(),
				BusinessUnitId = Guid.NewGuid().ToString()
			};
			var mailbox2 = new Subscription
			{
				MailboxId = mailbox2Id.ToString(),
				BusinessUnitId = Guid.NewGuid().ToString()
			};
			Server.AddMailbox(mailbox1);
			Server.AddMailbox(mailbox2);
			var notification = new Message
			{
				BusinessUnitId = mailbox2.BusinessUnitId
			};

			Server.NotifyClients(notification);

			Mailboxes.Data.Single(x => x.Id.Equals(mailbox1Id)).PopAllMessages().Should().Have.Count.EqualTo(0);
			Mailboxes.Data.Single(x => x.Id.Equals(mailbox2Id)).PopAllMessages().Single().Routes().Should().Have.SameValuesAs(notification.Routes());
		}

		[Test]
		public void ShouldReturnMessagesFromMailbox()
		{
			var subscription = new Subscription {MailboxId = Guid.NewGuid().ToString()};
			Server.AddMailbox(subscription);
			var notification = new Message();
			Server.NotifyClients(notification);

			var messages = Server.PopMessages(subscription.MailboxId);

			messages.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPopMessagesFromMailbox()
		{
			var subscription = new Subscription {MailboxId = Guid.NewGuid().ToString()};
			Server.AddMailbox(subscription);
			var notification = new Message();
			Server.NotifyClients(notification);

			Server.PopMessages(subscription.MailboxId);
			var messages = Server.PopMessages(subscription.MailboxId);

			messages.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldReturnNullWhenPoppingFromNonExistingMailbox()
		{
			Server.PopMessages(Guid.NewGuid().ToString())
				.Should().Be.Null();
		}

		[Test]
		public void ShouldPublish2MessagesToMailbox()
		{
			var mailbox = new Subscription
			{
				MailboxId = Guid.NewGuid().ToString(),
				BusinessUnitId = Guid.NewGuid().ToString()
			};
			Server.AddMailbox(mailbox);
			var notification = new Message
			{
				BusinessUnitId = mailbox.BusinessUnitId
			};

			Server.NotifyClients(notification);
			Server.NotifyClients(notification);

			Mailboxes.Data.Single().PopAllMessages().Should().Have.Count.EqualTo(2);
		}
	}
}

