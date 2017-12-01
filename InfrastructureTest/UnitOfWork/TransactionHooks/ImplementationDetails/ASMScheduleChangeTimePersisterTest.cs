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
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.IocCommon;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks.ImplementationDetails
{
	[TestFixture, DatabaseTest]
	public class ASMScheduleChangeTimePersisterTest : ISetup
	{
		public ASMScheduleChangeTimePersister Target;
		public FakeASMScheduleChangeTimeRepository Repo;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ASMScheduleChangeTimePersister>().For<ITransactionHook>();
			system.UseTestDouble<FakeASMScheduleChangeTimeRepository>().For<IASMScheduleChangeTimeRepository>();
			system.UseTestDouble<MutableNow>().For<INow>();
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
			var now = new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(now);
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
			var now = new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc);
			Now.Is(now);

			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(now);
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
			Now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null();
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
			Now.Is(new DateTime(2017, 11, 26, 20, 0, 0, DateTimeKind.Utc));
			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null();
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
			var now = new DateTime(2017, 11, 26, 20, 0, 0, DateTimeKind.Utc);
			Now.Is(now);
			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(now);
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
			Now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));

			Target.AfterCompletion(roots);

			var newTime = new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc);
			Now.Is(newTime);

			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(newTime);
		}

		[Test]
		public void ShouldUpdateScheduleChangeTimeForPersonAbsence()
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
			Now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));

			Target.AfterCompletion(roots);

			var newTime = new DateTime(2017, 11, 24, 15, 0, 0, DateTimeKind.Utc);
			Now.Is(newTime);

			Target.AfterCompletion(roots);

			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).TimeStamp.Should().Be(newTime);
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
			Now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			Target.AfterCompletion(roots);
			
			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null(); 
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
			Now.Is(new DateTime(2017, 11, 24, 13, 0, 0, DateTimeKind.Utc));
			Target.AfterCompletion(roots);
			Repo.GetScheduleChangeTime(person.Id.GetValueOrDefault()).Should().Be.Null();
		}


	}

}