using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
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

			Target.Load(mailbox.Id).Should().Be.EqualTo(mailbox);
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

			Target.Load(new []{mailbox.Route}).Single().Should().Be.EqualTo(mailbox);
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