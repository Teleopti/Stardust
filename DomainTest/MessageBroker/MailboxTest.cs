using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Messaging;

namespace Teleopti.Ccc.DomainTest.MessageBroker
{
	[TestFixture]
	[MessagingTest]
	[Setting("MessageBrokerMailboxExpirationInSeconds", 1000)]
	public class MailboxTest
	{
		public MessageBrokerServerTester Server;
		public FakeMailboxRepository Mailboxes;
		public MutableNow Now;

		[Test]
		public void ShouldAddMailbox()
		{
			var subscription = new Subscription();

			Server.PopMessages(subscription.Route(), Guid.NewGuid().ToString());

			Mailboxes.PersistedLast.Route.Should().Be(subscription.Route());
		}

		[Test]
		public void ShouldAddMailboxWithId()
		{
			var mailboxId = Guid.NewGuid();
			var subscription = new Subscription();

			Server.PopMessages(subscription.Route(), mailboxId.ToString());

			Mailboxes.PersistedLast.Id.Should().Be(mailboxId);
		}

		[Test]
		public void ShouldPublishToMailbox()
		{
			var mailbox = new Subscription
			{
				BusinessUnitId = Guid.NewGuid().ToString()
			};
			Server.PopMessages(mailbox.Route(), Guid.NewGuid().ToString());
			var notification = new Message
			{
				BusinessUnitId = mailbox.BusinessUnitId
			};

			Server.NotifyClients(notification);

			Mailboxes.Data.Single().Messages.Single().Routes().Should().Have.SameValuesAs(notification.Routes());
		}

		[Test]
		public void ShouldPublishToMailboxMatchingRoute()
		{
			var mailbox1Id = Guid.NewGuid();
			var mailbox2Id = Guid.NewGuid();
			var mailbox1 = new Subscription
			{
				BusinessUnitId = Guid.NewGuid().ToString()
			};
			var mailbox2 = new Subscription
			{
				BusinessUnitId = Guid.NewGuid().ToString()
			};
			Server.PopMessages(mailbox1.Route(), mailbox1Id.ToString());
			Server.PopMessages(mailbox2.Route(), mailbox2Id.ToString());
			var notification = new Message
			{
				BusinessUnitId = mailbox2.BusinessUnitId
			};

			Server.NotifyClients(notification);

			Mailboxes.Data.Single(x => x.Id.Equals(mailbox1Id)).Messages.Should().Be.Empty();
			Mailboxes.Data.Single(x => x.Id.Equals(mailbox2Id)).Messages.Single().Routes().Should().Have.SameValuesAs(notification.Routes());
		}

		[Test]
		public void ShouldReturnMessagesFromMailbox()
		{
			var subscription = new Subscription();
			var mailboxId = Guid.NewGuid();
			Server.PopMessages(subscription.Route(), mailboxId.ToString());
			var notification = new Message();
			Server.NotifyClients(notification);

			var messages = Server.PopMessages(subscription.Route(), mailboxId.ToString());

			messages.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPopMessagesFromMailbox()
		{
			var subscription = new Subscription();
			var mailboxId = Guid.NewGuid();
			Server.PopMessages(subscription.Route(), mailboxId.ToString());
			var notification = new Message();
			Server.NotifyClients(notification);

			Server.PopMessages(subscription.Route(), mailboxId.ToString());
			var messages = Server.PopMessages(subscription.Route(), mailboxId.ToString());

			messages.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldReturnEmptyArrayWhenPoppingFromNonExistingMailbox()
		{
			Server.PopMessages(new Subscription().Route(), Guid.NewGuid().ToString())
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublish2MessagesToMailbox()
		{
			var mailbox = new Subscription
			{
				BusinessUnitId = Guid.NewGuid().ToString()
			};
			Server.PopMessages(mailbox.Route(), Guid.NewGuid().ToString());
			var notification = new Message
			{
				BusinessUnitId = mailbox.BusinessUnitId
			};

			Server.NotifyClients(notification);
			Server.NotifyClients(notification);

			Mailboxes.Data.Single().Messages.Should().Have.Count.EqualTo(2);
		}
	}
}