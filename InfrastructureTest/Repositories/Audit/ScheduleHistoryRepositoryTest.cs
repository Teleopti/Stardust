using System;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Envers;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories.Audit
{
	[TestFixture]
	[Category("BucketB")]
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
		public void ShouldFindRevisionWithAbsenceOnNextDayIfUnderlyingAssignmentExtendsDay()
		{
			var expected = new[]
			{
				new Revision {Id = revisionNumberAfterOneUnitTestModification +1}, 
				new Revision {Id = revisionNumberAfterOneUnitTestModification}, 
				new Revision {Id = revisionNumberAtSetupStart},
			};
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddActivity(PersonAssignment.MainActivities().First().Payload, new DateTimePeriod(Today.AddHours(23), Today.AddHours(30)));
				PersonAssignmentRepository.DONT_USE_CTOR(new ThisUnitOfWork(uow)).Add(PersonAssignment);
				uow.PersistAll();
			}
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var newAbsence = new PersonAbsence(Agent, Scenario,
					new AbsenceLayer(PersonAbsence.Layer.Payload, new DateTimePeriod(Today.AddHours(27), Today.AddHours(28))));

				new PersonAbsenceRepository(new ThisUnitOfWork(uow)).Add(newAbsence);				
				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				target.FindRevisions(Agent, new DateOnly(Today), 3)
					.Should().Have.SameSequenceAs(expected);
			}
		}
		

		[Test]
		public void ShouldFindDistinctRevision()
		{
			var expected = new[] { new Revision { Id = revisionNumberAfterOneUnitTestModification }, new Revision { Id = revisionNumberAtSetupStart } };
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var absenceRepository = new PersonAbsenceRepository(new ThisUnitOfWork(uow));
				absenceRepository.Remove(PersonAbsence);
				absenceRepository.Add(PersonAbsence);
				uow.PersistAll();
			}

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var session = uow.FetchSession();
				session.CreateSQLQuery(
						"insert into Auditing.PersonAssignment_AUD (Id, REV, REVTYPE,[Version],Person,Scenario,ShiftCategory,[Date])" +
						"Values(NEWID(), :rev, 1, 1, :personId, :scenario, :shiftCategory, :date)")
					.SetInt32("rev", (int) revisionNumberAfterOneUnitTestModification)
					.SetGuid("personId", PersonAssignment.Person.Id.Value)
					.SetGuid("scenario", PersonAssignment.Scenario.Id.Value)
					.SetGuid("shiftCategory", PersonAssignment.ShiftCategory.Id.Value)
					.SetDateTime("date", new DateOnly(Today).Date)
					.ExecuteUpdate();

				session.CreateSQLQuery(
						"insert into Auditing.PersonAssignment_AUD (Id, REV, REVTYPE,[Version],Person,Scenario,ShiftCategory,[Date])" +
						"Values(NEWID(), :rev, 2, 1, :personId, :scenario, :shiftCategory, :date)")
					.SetInt32("rev", (int) revisionNumberAfterOneUnitTestModification)
					.SetGuid("personId", PersonAssignment.Person.Id.Value)
					.SetGuid("scenario", PersonAssignment.Scenario.Id.Value)
					.SetGuid("shiftCategory", PersonAssignment.ShiftCategory.Id.Value)
					.SetDateTime("date", new DateOnly(Today).Date)
					.ExecuteUpdate();

				uow.PersistAll();
			}

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var revIds = target.FindRevisions(Agent, new DateOnly(Today), 2);
				revIds.Count().Should().Be(2);
				revIds.Should().Have.SameSequenceAs(expected);
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
            PersonAssignment assignment;

            using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                assignment = target.FindSchedules(new Revision {Id = revisionNumberAtSetupStart}, Agent, new DateOnly(Today)).ToList()[0] as PersonAssignment;
            }

            var description = assignment.ShiftCategory.Description.Name;
            Assert.That(description, Is.Not.Null.And.Not.Empty);
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
				PersonAssignmentRepository.DONT_USE_CTOR(new ThisUnitOfWork(uow)).Remove(PersonAssignment);
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
				PersonAssignmentRepository.DONT_USE_CTOR(new ThisUnitOfWork(uow)).Add(PersonAssignment);

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
		public void ShouldThrowIfNotPositiveMaxResult()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => target.FindRevisions(Agent, new DateOnly(Today), 0));
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
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				revisionNumberAtSetupStart = uow.FetchSession().CreateCriteria<Revision>()
					.SetProjection(Projections.Max("Id"))
					.UniqueResult<long>();
				//insert empty revision for more realistic tests
				//DO NOT DO THIS IN REAL CODE!
				var dummyRevision = uow.FetchSession().Auditer().GetCurrentRevision<Revision>(true);
				dummyRevision.SetRevisionData(Agent);
				revisionNumberAfterOneUnitTestModification = revisionNumberAtSetupStart + 2;
			}
			target = new ScheduleHistoryRepository(UnitOfWorkFactory.CurrentUnitOfWork());
		}
	}
}