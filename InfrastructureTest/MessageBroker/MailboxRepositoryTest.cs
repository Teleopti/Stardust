﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	[TestFixture, Category("LongRunning")]
	[MessageBrokerUnitOfWorkTest]
	public class MailboxRepositoryTest
	{
		public IMailboxRepository Target;
		public ICurrentMessageBrokerUnitOfWork CurrentMessageBrokerUnitOfWork;

		[Test]
		public void ShouldGetFromId()
		{
			var notification = new Message { BusinessUnitId = Guid.NewGuid().ToString() };
			var mailbox = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = notification.Routes().First()
			};
			mailbox.AddMessage(notification);
			Target.Persist(mailbox);

			var result = Target.Load(mailbox.Id);

			result.Id.Should().Be(mailbox.Id);
			result.Route.Should().Be(mailbox.Route);
			result.Messages.Should().Have.Count.EqualTo(mailbox.Messages.Count());
			result.Messages.Single().BusinessUnitId.Should().Be(notification.BusinessUnitId);
		}

		[Test]
		public void ShouldGetFromRoutes()
		{
			var notification = new Message { BusinessUnitId = Guid.NewGuid().ToString() };
			var mailbox = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = notification.Routes().First()
			};
			mailbox.AddMessage(notification);
			Target.Persist(mailbox);

			var result = Target.Load(new []{mailbox.Route});

			result.Single().Id.Should().Be(mailbox.Id);
		}

		[Test]
		public void ShouldReturnNullWhenLoadingNonExistingMailbox()
		{
			Target.Load(Guid.NewGuid()).Should().Be.Null();
		}

		[Test]
		public void ShouldPersistAddedNotifications()
		{
			var businessUnitId = Guid.NewGuid().ToString();
			var mailboxId = Guid.NewGuid();
			Target.Persist(new Mailbox
			{
				Id = mailboxId,
				Route = new Message { BusinessUnitId = businessUnitId }.Routes().First()
			});
			var mailbox = Target.Load(mailboxId);
			mailbox.AddMessage(new Message { BusinessUnitId = businessUnitId });
			Target.Persist(mailbox);

			mailbox = Target.Load(mailboxId);
			mailbox.AddMessage(new Message { BusinessUnitId = businessUnitId });
			Target.Persist(mailbox);

			Target.Load(mailboxId).Messages.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldPersistAddedNotificationsLoadedByRoute()
		{
			var businessUnitId = Guid.NewGuid().ToString();
			var mailboxId = Guid.NewGuid();
			var route = new Message { BusinessUnitId = businessUnitId }.Routes().First();
			Target.Persist(new Mailbox
			{
				Id = mailboxId,
				Route = route
			});
			var mailbox = Target.Load(new[] {route}).Single();
			mailbox.AddMessage(new Message { BusinessUnitId = businessUnitId });
			Target.Persist(mailbox);

			mailbox = Target.Load(new[] {route}).Single();
			mailbox.AddMessage(new Message { BusinessUnitId = businessUnitId });
			Target.Persist(mailbox);

			Target.Load(mailboxId).Messages.Should().Have.Count.EqualTo(2);
		}

	}
}