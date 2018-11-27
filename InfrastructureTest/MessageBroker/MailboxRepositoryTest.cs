using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	[TestFixture]
	[MessageBrokerUnitOfWorkTest]
	public class MailboxRepositoryTest
	{
		public IMailboxRepository Target;
		public ICurrentMessageBrokerUnitOfWork UnitOfWork;
		public MutableNow Now;

		[Test]
		public void ShouldLoadById()
		{
			var notification = new Message {BusinessUnitId = Guid.NewGuid().ToString()};
			var mailbox = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = notification.Routes().First()
			};
			Target.Add(mailbox);

			var result = Target.Load(mailbox.Id);

			result.Id.Should().Be(mailbox.Id);
			result.Route.Should().Be(mailbox.Route);
		}

		[Test]
		public void ShouldReturnNullWhenLoadingNonExistingMailbox()
		{
			Target.Load(Guid.NewGuid()).Should().Be.Null();
		}

		[Test]
		public void ShouldPopMessages()
		{
			var notification = new Message {BusinessUnitId = Guid.NewGuid().ToString()};
			var mailbox = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = notification.Routes().First()
			};
			Target.Add(mailbox);
			Target.AddMessage(notification);

			var result = Target.PopMessages(mailbox.Id, null);

			result.Should().Have.Count.EqualTo(1);
			result.Single().BusinessUnitId.Should().Be(notification.BusinessUnitId);
		}

		[Test]
		public void ShouldAddNotifications()
		{
			var businessUnitId = Guid.NewGuid().ToString();
			var mailboxId = Guid.NewGuid();
			Target.Add(new Mailbox
			{
				Id = mailboxId,
				Route = new Message {BusinessUnitId = businessUnitId}.Routes().First()
			});
			Target.AddMessage(new Message {BusinessUnitId = businessUnitId});
			Target.AddMessage(new Message {BusinessUnitId = businessUnitId});

			Target.PopMessages(mailboxId, null).Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldPersistExpireAt()
		{
			var mailbox = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = new Message {BusinessUnitId = Guid.NewGuid().ToString()}.Routes().First(),
				ExpiresAt = "2015-06-26 08:30".Utc()
			};

			Target.Add(mailbox);

			var result = Target.Load(mailbox.Id);
			result.ExpiresAt.Should().Be("2015-06-26 08:30".Utc());
		}

		[Test]
		public void ShouldPersistExpireAtOnPop()
		{
			var mailbox = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = new Message {BusinessUnitId = Guid.NewGuid().ToString()}.Routes().First()
			};
			Target.Add(mailbox);

			Target.PopMessages(mailbox.Id, "2015-06-26 08:30".Utc());

			var result = Target.Load(mailbox.Id);
			result.ExpiresAt.Should().Be("2015-06-26 08:30".Utc());
		}

		[Test]
		public void ShouldPurgeExpiredMailbox()
		{
			var businessUnitId = Guid.NewGuid().ToString();
			var mailbox = new Mailbox
			{
				Id = Guid.NewGuid(),
				ExpiresAt = "2015-06-26 08:30".Utc(),
				Route = new Message {BusinessUnitId = businessUnitId}.Routes().First()
			};
			Target.Add(mailbox);
			Target.AddMessage(new Message {BusinessUnitId = businessUnitId});

			Now.Is("2015-06-26 08:30");
			Target.PurgeOneChunkOfMailboxes();
			Target.PurgeOneChunkOfNotifications();

			Target.Load(mailbox.Id).Should().Be.Null();
			Target.PopMessages(mailbox.Id, null).Should().Be.Empty();
		}

		[Test]
		public void ShouldHandleLargeMessages()
		{
			var message = new Message
			{
				BusinessUnitId = Guid.NewGuid().ToString(),
				BinaryData = string.Join("|", Enumerable.Range(0, 1000)
					.Select(_ => Guid.NewGuid().ToString()))
			};
			var mailbox = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = message.Routes().First()
			};
			Target.Add(mailbox);

			Assert.DoesNotThrow(() => { Target.AddMessage(message); });
		}
	}
}