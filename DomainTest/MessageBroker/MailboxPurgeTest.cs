using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.DomainTest.MessageBroker
{
	[TestFixture]
	[MessageBrokerServerTest]
	[Setting("MessageBrokerMailboxExpirationInSeconds", "1800")]
	public class MailboxPurgeTest
	{
		public IMessageBrokerServer Server;
		public FakeMailboxRepository Mailboxes;
		public MutableNow Now;

		[Test]
		public void ShouldPurgeEvery5Minutes()
		{
			Now.Is("2015-06-26 08:00");
			Server.NotifyClients(new Message());
			Mailboxes.PurgeWasCalled.Should().Be.True();
			Mailboxes.PurgeWasCalled = false;

			Now.Is("2015-06-26 08:04:59");
			Server.NotifyClients(new Message());
			Mailboxes.PurgeWasCalled.Should().Be.False();

			Now.Is("2015-06-26 08:05:00");
			Server.NotifyClients(new Message());
			Mailboxes.PurgeWasCalled.Should().Be.True();
			Mailboxes.PurgeWasCalled = false;

			Now.Is("2015-06-26 08:09:59");
			Server.NotifyClients(new Message());
			Mailboxes.PurgeWasCalled.Should().Be.False();

			Now.Is("2015-06-26 08:10:00");
			Server.NotifyClients(new Message());
			Mailboxes.PurgeWasCalled.Should().Be.True();
			Mailboxes.PurgeWasCalled = false;

			Now.Is("2015-06-26 09:00:00");
			Server.NotifyClients(new Message());
			Mailboxes.PurgeWasCalled.Should().Be.True();
			Mailboxes.PurgeWasCalled = false;

			Server.NotifyClients(new Message());
			Mailboxes.PurgeWasCalled.Should().Be.False();
		}

		[Test]
		public void ShouldPurgeMailboxesNotPoppedWithin30Minutes()
		{
			Now.Is("2015-06-26 08:00");
			Server.PopMessages(new Subscription().Route(), Guid.NewGuid().ToString());

			Now.Is("2015-06-26 08:31");
			Server.NotifyClients(new Message());

			Mailboxes.Data.Should().Be.Empty();
		}

		[Test]
		public void ShouldMaybeNotDeleteMailboxWhenLineIsQuiet()
		{
			var mailboxId = Guid.NewGuid().ToString();
			Now.Is("2015-06-26 08:00");
			Server.PopMessages(new Subscription().Route(), mailboxId);
			Now.Is("2015-06-26 08:28");
			Server.PopMessages(new Subscription().Route(), mailboxId);

			Now.Is("2015-06-26 08:31");
			Server.NotifyClients(new Message());

			Mailboxes.Data.Single().Messages.Should().Not.Be.Empty();
		}
	}
}