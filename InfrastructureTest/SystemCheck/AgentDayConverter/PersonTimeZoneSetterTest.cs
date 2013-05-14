using System;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	[TestFixture]
	public class PersonTimeZoneSetterTest : DatabaseTestWithoutTransaction
	{
		private IPerson snubbe;

		protected override void SetupForRepositoryTestWithoutTransaction()
		{
			snubbe = new Person();
			snubbe.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PersistAndRemoveFromUnitOfWork(snubbe);
			UnitOfWork.PersistAll();
		}

		[Test]
		public void ShouldPersistTimeZone()
		{
			var newTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			new PersonTimeZoneSetter().ExecuteConverterAndWrapInTransaction(snubbe.Id.Value, newTimeZone);

			new PersonRepository(UnitOfWork).Get(snubbe.Id.Value).PermissionInformation.DefaultTimeZone().Id
			                                .Should().Be.EqualTo(newTimeZone.Id);
		}

		[Test]
		public void ShouldNotPersistTimeZoneForWrongPerson()
		{
			var oldTimeZone = snubbe.PermissionInformation.DefaultTimeZone();

			new PersonTimeZoneSetter().ExecuteConverterAndWrapInTransaction(Guid.NewGuid(),
			                                                                TimeZoneInfo.FindSystemTimeZoneById(
				                                                                "Eastern Standard Time"));

			new PersonRepository(UnitOfWork).Get(snubbe.Id.Value).PermissionInformation.DefaultTimeZone()
			                                .Should().Be.EqualTo(oldTimeZone);
		}

		[Test]
		public void ShouldSetAssignmentForPersonToDefaultDate()
		{
			var ass = createAndStoreAssignment(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			PersistAndRemoveFromUnitOfWork(ass);

			new PersonTimeZoneSetter().ExecuteConverterAndWrapInTransaction(snubbe.Id.Value,
			                          TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

			new PersonAssignmentRepository(UnitOfWork).Get(ass.Id.Value).Date
			                                          .Should().Be.EqualTo(AgentDayConverters.DateOfUnconvertedSchedule);
		}

		[Test]
		public void ShouldNotSetAssignmentForWrongPersonToDefaultDate()
		{
			var ass = createAndStoreAssignment(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			PersistAndRemoveFromUnitOfWork(ass);

			new PersonTimeZoneSetter().ExecuteConverterAndWrapInTransaction(Guid.NewGuid(),
			                          TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

			new PersonAssignmentRepository(UnitOfWork).Get(ass.Id.Value).Date
			                                          .Should().Not.Be.EqualTo(AgentDayConverters.DateOfUnconvertedSchedule);
		}

		private IPersonAssignment createAndStoreAssignment(DateTime start)
		{
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("sdf"),
			                                                               snubbe,
			                                                               new DateTimePeriod(start, start.AddHours(8)),
			                                                               new ShiftCategory("d"), new Scenario("d"));
			PersistAndRemoveFromUnitOfWork(pa.MainShift.LayerCollection[0].Payload);
			PersistAndRemoveFromUnitOfWork(pa.ShiftCategory);
			PersistAndRemoveFromUnitOfWork(pa.Scenario);
			PersistAndRemoveFromUnitOfWork(pa);
			return pa;
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonRepository(uow).Remove(snubbe);
				var s = fetchSession(uow);
				s.CreateQuery("update Activity set IsDeleted=1").ExecuteUpdate();
				s.CreateQuery("update ShiftCategory set IsDeleted=1").ExecuteUpdate();
				s.CreateQuery("update Scenario set IsDeleted=1").ExecuteUpdate();
				uow.PersistAll();
			}
		}

		private static ISession fetchSession(IUnitOfWork uow)
		{
			return (ISession)typeof(NHibernateUnitOfWork).GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic)
															.GetValue(uow, null);
		}
	}
}