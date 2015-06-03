using System;
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
	public class MailboxRepositoryTest2
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
			result.Messages.Should().Have.Count.EqualTo(mailbox.Messages.Count);
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
		public void ShouldHandleMultipleNotifications()
		{
			var notification1 = new Message { BusinessUnitId = Guid.NewGuid().ToString() };
			var notification2 = new Message { BusinessUnitId = Guid.NewGuid().ToString() };
			var mailbox = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = notification1.Routes().First()
			};
			mailbox.AddMessage(notification1);
			mailbox.AddMessage(notification2);

			Assert.DoesNotThrow(() => Target.Persist(mailbox));
		}

		[Test]
		public void ShouldNotThrowWhenLoadingWithoutPersistedData()
		{
			Assert.DoesNotThrow(() => Target.Load(Guid.NewGuid()));
		}
	}
}