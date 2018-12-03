using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
	[Setting("MessageBrokerMailboxExpirationInSeconds", "1800")]
	public class MailboxPurgeTest
	{
		public IMessageBrokerServer Server;
		public MessageBrokerMailboxPurger Purger;
		public FakeMailboxRepository Mailboxes;
		public MutableNow Now;

		[Test]
		public void ShouldPurgeMailboxesNotPoppedWithin30Minutes()
		{
			Now.Is("2015-06-26 08:00");
			Server.PopMessages(new Subscription().Route(), Guid.NewGuid().ToString());
			Now.Is("2015-06-26 08:31");
			Server.NotifyClients(new Message());

			Purger.Handle(new SharedMinuteTickEvent());

			Mailboxes.Data.Should().Be.Empty();
		}
	}
}