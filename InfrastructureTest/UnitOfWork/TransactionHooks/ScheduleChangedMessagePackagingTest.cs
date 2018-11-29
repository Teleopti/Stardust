using System;
using System.Linq;
using System.Threading;
using NHibernate.Dialect;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks
{
	[TestFixture]
	[PrincipalAndStateTest]
	[Setting("ScheduleChangedMessagePackagingSendIntervalMilliseconds", 1000)]
	public class ScheduleChangedMessagePackagingTest
	{
		public FakeMessageSender MessageSender;
		public FakeTime Time;
		public Database Database;
		public ConcurrencyRunner Run;
		public ICurrentDataSource DataSource;
		public IInitiatorIdentifierScope Initiator;
		public IBusinessUnitScope BusinessUnit;
		public IPrincipalAndStateContext Context;

		[Test]
		public void ShouldSendMessage()
		{
			Database
				.WithPerson()
				.WithAssignment("2018-11-29")
				.WithAssignment("2018-11-30");
			Time.Passes("1".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPackageMessages()
		{
			Database
				.WithPerson()
				.WithAssignment("2018-11-29")
				.WithAssignment("2018-11-30");
			Time.Passes("2".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPackageMessagesForPeriod()
		{
			Database
				.WithPerson()
				.WithAssignedActivity("Phone", "2018-11-29 08:00", "2018-11-29 17:00")
				.WithAssignedActivity("Phone", "2018-11-30 08:00", "2018-11-30 17:00")
				;
			Time.Passes("2".Seconds());

			var message = MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Single();
			message.StartDateAsDateTime().Should().Be("2018-11-29 08:00".Utc());
			message.EndDateAsDateTime().Should().Be("2018-11-30 17:00".Utc());
			message.BinaryData.Should().Contain(Database.CurrentPersonId().ToString());
		}

		[Test]
		public void ShouldPackageMessagesForPersons()
		{
			Database
				.WithPerson()
				.WithAssignedActivity("Phone", "2018-11-29 08:00", "2018-11-29 17:00");
			var person1 = Database.CurrentPersonId();
			Database
				.WithPerson()
				.WithAssignedActivity("Phone", "2018-11-29 08:00", "2018-11-29 17:00")
				;
			var person2 = Database.CurrentPersonId();
			Time.Passes("1".Seconds());

			var message = MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Single();
			message.StartDateAsDateTime().Should().Be("2018-11-29 08:00".Utc());
			message.EndDateAsDateTime().Should().Be("2018-11-29 17:00".Utc());
			message.BinaryData.Should().Contain(person1.ToString());
			message.BinaryData.Should().Contain(person2.ToString());
		}

		[Test]
		public void ShouldPackageForEachThread()
		{
			Run.InParallel
			(() =>
				{
					Database
						.WithPerson()
						.WithAssignment("2018-11-29")
						.WithAssignment("2018-11-30");
				}
			);
			Run.Wait();
			Run.InParallel
			(() =>
				{
					Database
						.WithPerson()
						.WithAssignment("2018-11-29")
						.WithAssignment("2018-11-30");
				}
			);
			Run.Wait();
			Time.Passes("2".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldPackageMessagesForEachInitiator()
		{
			var initiator1 = new InitiatorIdentifier {InitiatorId = Guid.NewGuid()};
			using (Initiator.OnThisThreadUse(initiator1))
				Database
					.WithPerson()
					.WithAssignedActivity("Phone", "2018-11-29 08:00", "2018-11-29 17:00");
			var initiator2 = new InitiatorIdentifier {InitiatorId = Guid.NewGuid()};
			using (Initiator.OnThisThreadUse(initiator2))
				Database
					.WithAssignedActivity("Phone", "2018-11-30 08:00", "2018-11-30 17:00");
			Time.Passes("1".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>()
				.Select(x => x.ModuleId).Should().Have.SameValuesAs(initiator1.InitiatorId.ToString(), initiator2.InitiatorId.ToString());
		}

		[Test]
		public void ShouldPackageMessagesForEachDataSource()
		{
			var dataSource = DataSource.CurrentName();
			Database
				.WithPerson()
				.WithAssignedActivity("Phone", "2018-11-29 08:00", "2018-11-29 17:00");
			Context.Logout();
			Time.Passes("1".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().First()
				.DataSource.Should().Be(dataSource);
		}

		[Test]
		public void ShouldPackageMessagesForEachBusinessUnit()
		{
			Database
				.WithPerson()
				.WithActivity("Phone")
				.WithDefaultScenario("_");
			var businessUnit1 = new BusinessUnit("_");
			businessUnit1.SetId(Guid.NewGuid());
			using (BusinessUnit.OnThisThreadUse(businessUnit1))
				Database.WithAssignment("2018-11-29");
			var businessUnit2 = new BusinessUnit("_");
			businessUnit2.SetId(Guid.NewGuid());
			using (BusinessUnit.OnThisThreadUse(businessUnit2))
				Database.WithAssignment("2018-11-30");
			Time.Passes("1".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Select(x => x.BusinessUnitId)
				.Should().Have.SameValuesAs(businessUnit1.Id.Value.ToString(), businessUnit2.Id.Value.ToString());
		}

		[Test]
		public void ShouldPackageMessagesForEachScenario()
		{
			Database
				.WithPerson()
				.WithScenario("scenario1", true)
				.WithAssignedActivity("Phone", "2018-11-29 08:00", "2018-11-29 17:00");
			var scenario1 = Database.CurrentScenarioId();
			Database
				.WithScenario("scenario2", false)
				.WithAssignedActivity("Phone", "2018-11-30 08:00", "2018-11-30 17:00");
			var scenario2 = Database.CurrentScenarioId();

			Time.Passes("1".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Select(x => x.DomainReferenceId)
				.Should().Have.SameValuesAs(scenario1.ToString(), scenario2.ToString());
		}
	}
}