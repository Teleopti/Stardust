using System;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	[TestFixture]
	[AnalyticsDatabaseTest]
	public class MailboxConcurrencyTest
	{
		public IMessageBrokerServer Server;
		public MessageBrokerMailboxPurger Purger;

		[Test]
		public void ShouldNotBreakWhenPolling()
		{
			var run = new ConcurrencyRunner();
			var route = new Message {BusinessUnitId = Guid.NewGuid().ToString()}.Routes().First();

			100.Times(() =>
			{
				var mailboxId = Guid.NewGuid().ToString();
				run.InParallel(() => Server.PopMessages(route, mailboxId))
					.Times(5);
			});

			Assert.DoesNotThrow(() => run.WaitForException<SqlException>());
		}

		[Test]
		[Setting("MessageBrokerMailboxPurgeIntervalInSeconds", 0)]
		[Setting("MessageBrokerMailboxExpirationInSeconds", 0)]
		[Category("LongRunning")]
		public void ShouldNotDeadlockWhenPurging()
		{
			var run = new ConcurrencyRunner();

			var messages = Enumerable.Range(0, 1000)
				.Select(i => new Message {BusinessUnitId = Guid.NewGuid().ToString()})
				.ToArray();

			run.InParallel(() =>
			{
				Server.PopMessages(messages.GetRandom().Routes().First(), Guid.NewGuid().ToString());
				Server.NotifyClients(messages.GetRandom());
				Purger.Handle(new SharedMinuteTickEvent());
			}).Times(1000);

			Assert.DoesNotThrow(() => run.WaitForException<SqlException>());
		}
	}
}