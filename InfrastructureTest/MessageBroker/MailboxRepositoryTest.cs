using System;
using System.Linq;
using NHibernate.Transform;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	[TestFixture, Category("LongRunning")]
	[MessageBrokerUnitOfWorkTest]
	[Toggle(Toggles.MessageBroker_Mailbox_32733)]
	public class MailboxRepositoryTest
	{
		public IMailboxRepository Target;
		public ICurrentMessageBrokerUnitOfWork CurrentMessageBrokerUnitOfWork;
		public IJsonSerializer Serializer;

		[Test]
		public void ShouldPersist()
		{
			var id = Guid.NewGuid();
			var notification = new Notification {BusinessUnitId = Guid.NewGuid().ToString()};
			var mailbox = new Mailbox
			{
				Id = id,
				Route = notification.Routes().First(),
				Notifications = new []{notification}
			};

			Target.Persist(mailbox);

			var result = CurrentMessageBrokerUnitOfWork.Current().CreateSqlQuery(
				@"SELECT * FROM dbo.Mailbox")
				.SetResultTransformer(Transformers.AliasToBean(typeof (mailboxThing)))
				.List<mailboxThing>()
				.First();

			result.Id.Should().Be(id);
			result.Route.Should().Be(notification.Routes().First());
		}

		[Test]
		public void ShouldPersistNotification()
		{
			var id = Guid.NewGuid();
			var notification = new Notification { BusinessUnitId = Guid.NewGuid().ToString() };
			var mailbox = new Mailbox
			{
				Id = id,
				Route = " ",
				Notifications = new[] { notification }
			};

			Target.Persist(mailbox);

			var result = CurrentMessageBrokerUnitOfWork.Current().CreateSqlQuery(
				string.Format(@"SELECT * FROM dbo.Notification WHERE Parent = '{0}'", mailbox.Id))
				.SetResultTransformer(Transformers.AliasToBean(typeof(notificationThing)))
				.List<notificationThing>()
				.First();

			result.Message.Should().Be(Serializer.SerializeObject(notification));
		}


		[Test]
		public void ShouldGet()
		{
			var id = Guid.NewGuid();
			var notification = new Notification { BusinessUnitId = Guid.NewGuid().ToString() };
			var mailbox = new Mailbox
			{
				Id = id,
				Route = notification.Routes().First(),
				Notifications = new[] { notification }
			};

			Target.Persist(mailbox);

			var result = Target.Get(id);

			result.Id.Should().Be(id);
			result.Route.Should().Be(mailbox.Route);
			Serializer.SerializeObject(result.Notifications.First())
				.Should()
				.Be(Serializer.SerializeObject(result.Notifications.First()));
		}

		[Test]
		public void ShouldPersistWithSaferMethod()
		{
			var id = Guid.NewGuid();
			var mailbox1 = new Mailbox
			{
				Id = id,
				Route = " "
			};
			var harmfulInputFromUser = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = string.Format("'); DELETE FROM dbo.Mailbox WHERE Id = '{0}' --", id)
			};

			Target.Persist(mailbox1);
			Target.Persist(harmfulInputFromUser);

			Target.Get(id)
				.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldPersistWithSaferMethod2()
		{
			var id = Guid.NewGuid();
			var mailbox1 = new Mailbox
			{
				Id = id,
				Route = " "
			};
			var harmfulInputFromUser = new Mailbox
			{
				Id = Guid.NewGuid(),
				Route = " ",
				Notifications = new[] { new Notification { BusinessUnitId = string.Format("'); DELETE FROM dbo.Mailbox WHERE dbo.Mailbox.Id = '{0}' ;--", id) }, }
			};


			Target.Persist(mailbox1);
			Target.Persist(harmfulInputFromUser);

			Target.Get(id)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUpdateMailbox()
		{
			var id = Guid.NewGuid();
			var mailbox = new Mailbox()
			{
				Id = id,
				Route = " "
			};
			Target.Persist(mailbox);
			mailbox.AddNotification(new Notification{BusinessUnitId = Guid.NewGuid().ToString()});

			Target.Persist(mailbox);

			var result = Target.Get(id);
			Serializer.SerializeObject(result.Notifications.First())
				.Should()
				.Be(Serializer.SerializeObject(result.Notifications.First()));
		}

		[Test]
		public void ShouldUpdateMailboxAfterPoppingMessages()
		{
			var id = Guid.NewGuid();
			var mailbox = new Mailbox()
			{
				Id = id,
				Route = " "
			};
			mailbox.AddNotification(new Notification { BusinessUnitId = Guid.NewGuid().ToString() });
			Target.Persist(mailbox);
			mailbox.PopAll();

			Target.Persist(mailbox);

			Target.Get(id).Notifications.Should().Be.Empty();
		}

		[Test]
		public void ShouldGetMailboxesFromRoute()
		{
			var notification = new Notification {BusinessUnitId = Guid.NewGuid().ToString()};
			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			var mailbox1 = new Mailbox()
			{
				Id = id1,
				Route = notification.Routes().First()
			};
			var mailbox2 = new Mailbox()
			{
				Id = id2,
				Route = notification.Routes().First()
			};
			Target.Persist(mailbox1);
			Target.Persist(mailbox2);

			Target.Get(notification.Routes().First())
				.Select(x => Serializer.SerializeObject(x))
				.Should().Have.SameValuesAs(new[] { Serializer.SerializeObject(mailbox1), Serializer.SerializeObject(mailbox2) });
		}

		private class mailboxThing
		{
			public Guid Id { get; set; }
			public string Route { get; set; }
		}

		private class notificationThing
		{
			public Guid Parent { get; set; }
			public string Message { get; set; }
		}
	}
}