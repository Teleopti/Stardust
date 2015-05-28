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
			var notification = new Notification { BusinessUnitId = Guid.NewGuid().ToString() };
			var mailbox = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = notification.Routes().First()
			};
			mailbox.AddNotification(notification);
			Target.Persist(mailbox);

			var result = Target.Load(mailbox.Id);

			result.Id.Should().Be(mailbox.Id);
			result.Route.Should().Be(mailbox.Route);
			result.Notifications.Should().Have.Count.EqualTo(mailbox.Notifications.Count);
			result.Notifications.Single().BusinessUnitId.Should().Be(notification.BusinessUnitId);
		}

		[Test]
		public void ShouldGetFromRoutes()
		{
			var notification = new Notification { BusinessUnitId = Guid.NewGuid().ToString() };
			var mailbox = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = notification.Routes().First()
			};
			mailbox.AddNotification(notification);
			Target.Persist(mailbox);

			var result = Target.Load(new []{mailbox.Route});

			result.Single().Id.Should().Be(mailbox.Id);
		}

		[Test]
		public void ShouldHandleMultipleNotifications()
		{
			var notification1 = new Notification { BusinessUnitId = Guid.NewGuid().ToString() };
			var notification2 = new Notification { BusinessUnitId = Guid.NewGuid().ToString() };
			var mailbox = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = notification1.Routes().First()
			};
			mailbox.AddNotification(notification1);
			mailbox.AddNotification(notification2);

			Assert.DoesNotThrow(() => Target.Persist(mailbox));
		}

		[Test]
		public void ShouldNotThrowWhenLoadingWithoutPersistedData()
		{
			Assert.DoesNotThrow(() => Target.Load(Guid.NewGuid()));
		}
	}
}