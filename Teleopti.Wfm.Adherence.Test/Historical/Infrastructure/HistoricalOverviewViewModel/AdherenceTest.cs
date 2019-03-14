using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Historical;
using Teleopti.Wfm.Adherence.Historical.Approval;
using Teleopti.Wfm.Adherence.States.Events;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Historical.Infrastructure.HistoricalOverviewViewModel
{
	[DatabaseTest]
	public class AdherenceTest
	{
		public HistoricalOverviewViewModelBuilder Target;
		public Database Database;
		public MutableNow Now;
		public IEventPublisher Publisher;
		public IRtaEventStoreSynchronizer Synchronizer;
		public WithUnitOfWork UnitOfWork;
		public Approval Approval;

		[Test]
		public void ShouldWorkAfterSynchronize()
		{
			Now.Is("2018-11-13 08:00");
			Database
				.WithActivity("Phone")
				.WithTeam()
				.WithAgent()
				.WithAssignment("2018-11-13")
				.WithAssignedActivity("Phone", "2018-11-13 08:00", "2018-11-13 17:00");
			var person = Database.CurrentPersonId();
			var team = Database.CurrentTeamId();
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = person,
				BelongsToDate = "2018-11-13".Date(),
				Timestamp = "2018-11-13 08:00".Utc(),
				Adherence = EventAdherence.In
			});
			Now.Is("2018-11-14 08:00");
			Synchronizer.Synchronize();

			var data = UnitOfWork.Get(() => Target.Build(null, new[] {team}).First());

			data.Agents.Single().Days.Last().Adherence.Should().Be("100");
		}

		[Test]
		public void ShouldWorkAfterApproval()
		{
			Now.Is("2018-11-13 08:00");
			Database
				.WithActivity("Phone")
				.WithTeam()
				.WithAgent()
				.WithAssignment("2018-11-13")
				.WithAssignedActivity("Phone", "2018-11-13 08:00", "2018-11-13 17:00");
			var person = Database.CurrentPersonId();
			var team = Database.CurrentTeamId();
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = person,
				BelongsToDate = "2018-11-13".Date(),
				Timestamp = "2018-11-13 08:00".Utc(),
				Adherence = EventAdherence.Out
			});
			Now.Is("2018-11-14 08:00");
			UnitOfWork.Do(() => Approval.ApproveAsInAdherence(
				new PeriodToApprove()
				{
					PersonId = person,
					StartTime = "2018-11-13 08:00".Utc(),
					EndTime = "2018-11-13 17:00".Utc()
				}
			));

			var data = UnitOfWork.Get(() => Target.Build(null, new[] {team}).First());

			data.Agents.Single().Days.Last().Adherence.Should().Be("100");
		}

		[Test]
		public void ShouldWorkAfterRemovalOfApproval()
		{
			Now.Is("2018-11-13 08:00");
			Database
				.WithActivity("Phone")
				.WithTeam()
				.WithAgent()
				.WithAssignment("2018-11-13")
				.WithAssignedActivity("Phone", "2018-11-13 08:00", "2018-11-13 17:00");
			var person = Database.CurrentPersonId();
			var team = Database.CurrentTeamId();
			Publisher.Publish(new PersonStateChangedEvent
			{
				PersonId = person,
				BelongsToDate = "2018-11-13".Date(),
				Timestamp = "2018-11-13 08:00".Utc(),
				Adherence = EventAdherence.Out
			});
			Now.Is("2018-11-14 08:00");
			UnitOfWork.Do(() => Approval.ApproveAsInAdherence(
				new PeriodToApprove
				{
					PersonId = person,
					StartTime = "2018-11-13 08:00".Utc(),
					EndTime = "2018-11-13 17:00".Utc()
				}
			));
			UnitOfWork.Do(() => Approval.Cancel(
				new PeriodToCancel
				{
					PersonId = person,
					StartTime = "2018-11-13 08:00".Utc(),
					EndTime = "2018-11-13 17:00".Utc()
				}
			));

			var data = UnitOfWork.Get(() => Target.Build(null, new[] {team}).First());

			data.Agents.Single().Days.Last().Adherence.Should().Be("0");
		}
	}
}