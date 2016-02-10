using System;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Envers;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Audit
{
	[TestFixture]
	[Category("LongRunning")]
	public class ScheduleHistoryRepositoryTest : AuditTest
	{
		private IScheduleHistoryRepository target;
		private long revisionNumberAtSetupStart;
		private long revisionNumberAfterOneUnitTestModification;

		[Test]
		public void ShouldFindAssignmentInRevision()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindSchedules(new Revision { Id = revisionNumberAtSetupStart }, Agent, new DateOnly(Today))
					.Should().Contain(PersonAssignment);
			}
		}

		[Test]
		public void ShouldFindAbsenceInRevision()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindSchedules(new Revision { Id = revisionNumberAtSetupStart }, Agent, new DateOnly(Today))
					.Should().Contain(PersonAbsence);
			}
		}

		[Test]
		public void ShouldFindRevisionForModifiedAssignment()
		{
			var expected = new[] { new Revision { Id = revisionNumberAfterOneUnitTestModification }, new Revision { Id = revisionNumberAtSetupStart } };
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(PersonAssignment);
				PersonAssignment.AddActivity(PersonAssignment.MainActivities().First().Payload, PersonAssignment.MainActivities().First().Period);
				uow.PersistAll();
			}
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindRevisions(Agent, new DateOnly(Today), 2)
					.Should().Have.SameSequenceAs(expected);
			}
		}

		[Test]
		public void ShouldFindRevisionForDeletedAbsence()
		{
			var expected = new[] { new Revision { Id = revisionNumberAfterOneUnitTestModification }, new Revision { Id = revisionNumberAtSetupStart } };
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonAbsenceRepository(new ThisUnitOfWork(uow)).Remove(PersonAbsence);
				uow.PersistAll();
			}
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindRevisions(Agent, new DateOnly(Today), 2)
					.Should().Have.SameSequenceAs(expected);
			}
		}

		[Test]
		public void ShouldNotFindSchedulesInTheFuture()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindSchedules(new Revision { Id = revisionNumberAtSetupStart }, Agent, new DateOnly(Today.AddDays(2)))
					.Should().Be.Empty();
			}
		}

        // Check for lazy loading error: #33575
        [Test]
        public void ShouldShiftCategoryAlsoBeLoadedInFoundPersonAssingments()
        {
            PersonAssignment assignment = null;

            using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                assignment = target.FindSchedules(new Revision {Id = revisionNumberAtSetupStart}, Agent, new DateOnly(Today)).ToList()[0] as PersonAssignment;
            }

            var description = assignment.ShiftCategory.Description.Name;
            Assert.IsNotNullOrEmpty(description);
        }

		[Test]
		public void ShouldNotFindSchedulesInThePast()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindSchedules(new Revision { Id = revisionNumberAtSetupStart }, Agent, new DateOnly(Today.AddDays(-2)))
					.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldNotFindSchedulesForWrongPerson()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var dummyAgent = PersonFactory.CreatePerson();
				dummyAgent.SetId(Guid.NewGuid());
				target.FindSchedules(new Revision { Id = revisionNumberAtSetupStart }, dummyAgent, new DateOnly(Today))
					.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldFindOldButStillLatestScheduleDataWhenAssignmentIsRemoved()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonAssignmentRepository(uow).Remove(PersonAssignment);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindSchedules(new Revision { Id = revisionNumberAfterOneUnitTestModification }, Agent, new DateOnly(Today))
					.Should().Have.SameValuesAs(PersonAbsence);
			}
		}

		[Test]
		public void ShouldFindOldButStillLatestScheduleDataWhenAbsenceIsRemoved()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonAbsenceRepository(new ThisUnitOfWork(uow)).Remove(PersonAbsence);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindSchedules(new Revision { Id = revisionNumberAfterOneUnitTestModification }, Agent, new DateOnly(Today))
					.Should().Have.SameValuesAs(PersonAssignment);
			}
		}

		[Test]
		public void ShouldNotFindRevisionsForWrongPerson()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var dummyAgent = PersonFactory.CreatePerson();
				dummyAgent.SetId(Guid.NewGuid());
				target.FindRevisions(dummyAgent, new DateOnly(Today), 10)
					.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldNotFindRevisionsTooEarly()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindRevisions(Agent, new DateOnly(Today).AddDays(-1), 10)
					.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldNotFindRevisionsTooLate()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindRevisions(Agent, new DateOnly(Today).AddDays(1), 10)
					.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldNotFindMoreRevisionsThanSpecified()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var nu = DateTime.UtcNow.Date;
				var newAbsence = new PersonAbsence(Agent, Scenario,
				                                   new AbsenceLayer(PersonAbsence.Layer.Payload,
				                                                    new DateTimePeriod(nu.AddDays(3), nu.AddDays(4))));
				new PersonAbsenceRepository(new ThisUnitOfWork(uow)).Add(newAbsence);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindRevisions(Agent, new DateOnly(Today), 1)
					.Should().Have.SameSequenceAs(new Revision { Id = revisionNumberAtSetupStart });
			}
		}

		[Test]
		public void ShouldFindAbsenceNextDayDueToPossibleNightShifts()
		{
			var newAbsence = new PersonAbsence(Agent, Scenario,
					new AbsenceLayer(PersonAbsence.Layer.Payload, new DateTimePeriod(Today.AddHours(27), Today.AddHours(28))));
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddActivity(PersonAssignment.MainActivities().First().Payload, new DateTimePeriod(Today.AddHours(23), Today.AddHours(30)));
				new PersonAssignmentRepository(new ThisUnitOfWork(uow)).Add(PersonAssignment);

				new PersonAbsenceRepository(new ThisUnitOfWork(uow)).Add(newAbsence);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var data = target.FindSchedules(new Revision { Id = revisionNumberAfterOneUnitTestModification }, Agent, new DateOnly(Today));
				data.Should().Contain(newAbsence);
			}
		}

		[Test]
		public void ShouldNotFindAbsenceNextDayIfNoNightShifts()
		{
			var newAbsence = new PersonAbsence(Agent, Scenario,
					new AbsenceLayer(PersonAbsence.Layer.Payload, new DateTimePeriod(Today.AddHours(27), Today.AddHours(28))));
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonAbsenceRepository(new ThisUnitOfWork(uow)).Add(newAbsence);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var data = target.FindSchedules(new Revision { Id = revisionNumberAfterOneUnitTestModification }, Agent, new DateOnly(Today));
				data.Should().Not.Contain(newAbsence);
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ShouldThrowIfNotPositiveMaxResult()
		{
			target.FindRevisions(Agent, new DateOnly(Today), 0);
		}

		[Test]
		public void ShouldConsiderAgentTimeZone()
		{
			//utc +3.00 GMT
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabic Standard Time");
			var arabicTimeZone = (timeZone);

			//adding absence too early
			var period = new DateTimePeriod(Today.AddHours(-5), Today.AddHours(-2));
			var newAbs = new PersonAbsence(Agent, Scenario, new AbsenceLayer(PersonAbsence.Layer.Payload, period));

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonAbsenceRepository(new ThisUnitOfWork(uow)).Add(newAbs);
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindRevisions(Agent, new DateOnly(Today), 5)
					.Should().Have.Count.EqualTo(1);
				target.FindSchedules(new Revision {Id = revisionNumberAfterOneUnitTestModification}, Agent, new DateOnly(Today))
					.Should().Not.Contain(newAbs);

				Agent.PermissionInformation.SetDefaultTimeZone(arabicTimeZone);
				target.FindRevisions(Agent, new DateOnly(Today), 5)
					.Should().Have.Count.EqualTo(2);
				target.FindSchedules(new Revision { Id = revisionNumberAfterOneUnitTestModification }, Agent, new DateOnly(Today))
					.Should().Contain(newAbs);

			}
		}

		protected override void AuditSetup()
		{
			using (var tempSession = Session.SessionFactory.OpenSession())
			{
				using (var tx = tempSession.BeginTransaction())
				{
					revisionNumberAtSetupStart = tempSession.CreateCriteria<Revision>()
														.SetProjection(Projections.Max("Id"))
														.UniqueResult<long>();
					//insert empty revision for more realistic tests
					//DO NOT DO THIS IN REAL CODE!
					var dummyRevision = tempSession.Auditer().GetCurrentRevision<Revision>(true);
					dummyRevision.SetRevisionData(Agent);
					tx.Commit();
					revisionNumberAfterOneUnitTestModification = revisionNumberAtSetupStart + 2;
				}
			}
			target = new ScheduleHistoryRepository(UnitOfWorkFactory.Current);
		}
	}
}