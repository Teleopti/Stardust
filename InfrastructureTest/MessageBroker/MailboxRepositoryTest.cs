using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.MessageBroker;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	[TestFixture]
	[InfrastructureTest]
    public class MailboxRepositoryTest
	{
		public MailboxRepository Target;

		[Test, Ignore]
		public void ShouldPersist()
		{
			var mailbox = new Mailbox {Id = Guid.NewGuid()};
			Target.Persist(mailbox);

			Target.Get(mailbox.Id).Id.Should().Be.EqualTo(mailbox.Id);
		}
    }
}
