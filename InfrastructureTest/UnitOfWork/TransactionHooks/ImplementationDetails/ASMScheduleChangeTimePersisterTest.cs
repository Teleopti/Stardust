using NUnit.Framework;
using Rhino.Mocks;
using System;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks.ImplementationDetails
{
	[TestFixture]
	public class ASMScheduleChangeTimePersisterTest
	{
		private ASMScheduleChangeTimePersister _target;
		private IASMScheduleChangeTimeRepository _repo;
		private MutableNow _now;

		[SetUp]
		public void Setup()
		{
			_repo = MockRepository.GenerateMock<IASMScheduleChangeTimeRepository>();
			_now = new MutableNow();
			var currentBu = new FakeCurrentBusinessUnit();
			currentBu.FakeBusinessUnit(BusinessUnitFactory.CreateWithId(Guid.NewGuid()));
			_target = new ASMScheduleChangeTimePersister(_repo, _now, currentBu, new FakeCurrentDatasource("fake"));
		}
		[Test]
		public void ShouldAddScheduleChangeTimeForPersonAssignment()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 11, 24, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc)));
			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			_now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			var time = new ASMScheduleChangeTime
			{
				PersonId = person.Id.GetValueOrDefault(),
				TimeStamp = _now.UtcDateTime()
			};

			_target.Persist(roots);

			_repo.AssertWasCalled(x => x.Add(Arg<ASMScheduleChangeTime>.Matches(t => t.TimeStamp == time.TimeStamp && t.PersonId == time.PersonId)));
		}


		[Test]
		public void ShouldAddScheduleChangeTimeForPersonAbsence()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var period = new DateTimePeriod(new DateTime(2017, 11, 24, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period);


			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(personAbsence, DomainUpdateType.Update)
			};
			_now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			var time = new ASMScheduleChangeTime
			{
				PersonId = person.Id.GetValueOrDefault(),
				TimeStamp = _now.UtcDateTime()
			};

			_target.Persist(roots);

			_repo.AssertWasCalled(x => x.Add(Arg<ASMScheduleChangeTime>.Matches(t => t.TimeStamp == time.TimeStamp && t.PersonId == time.PersonId)));

		}

		[Test]
		public void ShouldNotAddScheduleChangeTimeIfTheTypeIsNotAbsenceOrAssignment()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var roots = new IRootChangeInfo[]
			{
						new RootChangeInfo(person, DomainUpdateType.Update)
			};
			_now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			var time = new ASMScheduleChangeTime
			{
				PersonId = person.Id.GetValueOrDefault(),
				TimeStamp = _now.UtcDateTime()
			};

			_target.Persist(roots);
			_repo.AssertWasNotCalled(x => x
				.Add(Arg<ASMScheduleChangeTime>
				.Matches(t => t.TimeStamp == time.TimeStamp && t.PersonId == time.PersonId)));

		}


		[Test]
		public void ShouldNotAddScheduleChangeTimeForPersonAssignmentIfNotInASMNotifyPeriod()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 11, 25, 10, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 25, 15, 0, 0, DateTimeKind.Utc)));

			var roots = new IRootChangeInfo[]
			{
								new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			_now.Is(new DateTime(2017, 11, 26, 20, 0, 0, DateTimeKind.Utc));
			var time = new ASMScheduleChangeTime
			{
				PersonId = person.Id.GetValueOrDefault(),
				TimeStamp = _now.UtcDateTime()
			};
			_target.Persist(roots);
			_repo.AssertWasNotCalled(x => x
							.Add(Arg<ASMScheduleChangeTime>
							.Matches(t => t.TimeStamp == time.TimeStamp && t.PersonId == time.PersonId)));
		}

		[Test]
		public void ShouldAddScheduleChangeTimeForPersonAssignmentIfJustInASMNotifyPeriod()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 11, 25, 10, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 25, 16, 0, 0, DateTimeKind.Utc)));

			var roots = new IRootChangeInfo[]
			{
								new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			_now.Is(new DateTime(2017, 11, 26, 20, 0, 0, DateTimeKind.Utc));
			var time = new ASMScheduleChangeTime
			{
				PersonId = person.Id.GetValueOrDefault(),
				TimeStamp = _now.UtcDateTime()
			};

			_target.Persist(roots);

			_repo.AssertWasCalled(x => x
						.Add(Arg<ASMScheduleChangeTime>
						.Matches(t => t.TimeStamp == time.TimeStamp && t.PersonId == time.PersonId)));
		}

		[Test]
		public void ShouldUpdateScheduleChangeTimeForPersonAssignment()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("default", true),
				new DateTimePeriod(new DateTime(2017, 11, 24, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc)));
			var roots = new IRootChangeInfo[]
			{
								new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			_now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			var time = new ASMScheduleChangeTime
			{
				PersonId = person.Id.GetValueOrDefault(),
				TimeStamp = _now.UtcDateTime()
			};
			_repo.Stub(x => x.GetScheduleChangeTime(Arg<Guid>
						.Matches(t => t == time.PersonId))).Return(time);

			var newTime = new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc);
			_now.Is(newTime);

			_target.Persist(roots);

			_repo.AssertWasCalled(x => x
						.Update(Arg<ASMScheduleChangeTime>
						.Matches(t => t.TimeStamp == newTime && t.PersonId == time.PersonId)));
		}

		[Test]
		public void ShouldUpdateScheduleChangeTimeForPersonAbsence() {
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var period = new DateTimePeriod(new DateTime(2017, 11, 24, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period);

			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(personAbsence, DomainUpdateType.Update)
			};
			_now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			var time = new ASMScheduleChangeTime
			{
				PersonId = person.Id.GetValueOrDefault(),
				TimeStamp = _now.UtcDateTime()
			};

			_repo.Stub(x => x.GetScheduleChangeTime(Arg<Guid>
						.Matches(t => t == time.PersonId))).Return(time);

			var newTime = new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc);
			_now.Is(newTime);

			_target.Persist(roots);

			_repo.AssertWasCalled(x => x
					.Update(Arg<ASMScheduleChangeTime>
					.Matches(t => t.TimeStamp == newTime && t.PersonId == time.PersonId)));
		}
		[Test]
		public void ShouldNotUpdateScheduleChangeTimeIfTheTypeIsNotAbsenceOrAssignment()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);

			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var roots = new IRootChangeInfo[]
			{
						new RootChangeInfo(person, DomainUpdateType.Update)
			};
			_now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			var time = new ASMScheduleChangeTime
			{
				PersonId = person.Id.GetValueOrDefault(),
				TimeStamp = _now.UtcDateTime()
			};

			_repo.Stub(x => x.GetScheduleChangeTime(Arg<Guid>
						.Matches(t => t == time.PersonId))).Return(time);

			var newTime = new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc);
			_now.Is(newTime);

			_target.Persist(roots);
			_repo.AssertWasNotCalled(x => x
					.Update(Arg<ASMScheduleChangeTime>
					.Matches(t => t.TimeStamp == newTime && t.PersonId == time.PersonId)));

		}


		[Test]
		public void ShouldNotAddScheduleChangeTimeForPersonAssignmentIfNotInDefaultScenario()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				ScenarioFactory.CreateScenarioWithId("not default", false),
				new DateTimePeriod(new DateTime(2017, 11, 24, 6, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc)));
			var roots = new IRootChangeInfo[]
			{
				new RootChangeInfo(pa, DomainUpdateType.Update)
			};
			_now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			var time = new ASMScheduleChangeTime
			{
				PersonId = person.Id.GetValueOrDefault(),
				TimeStamp = _now.UtcDateTime()
			};

			_target.Persist(roots);

			_repo.AssertWasNotCalled(x => x.Add(Arg<ASMScheduleChangeTime>.Matches(t => t.TimeStamp == time.TimeStamp && t.PersonId == time.PersonId)));

		}

	}

}