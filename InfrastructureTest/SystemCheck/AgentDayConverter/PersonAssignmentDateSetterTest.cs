﻿using System;
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
	public class PersonAssignmentDateSetterTest : DatabaseTestWithoutTransaction
	{
		[Test]
		public void ShouldSetCorrectDateForResettedPersonAssignment()
		{
			var paRep = new PersonAssignmentRepository(UnitOfWork);
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expected = new DateOnly(2000, 1, 1);

			var pa = createAndStoreAssignment(start);

			Session.ResetDateForAllAssignmentsAndAudits();
			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			paRep.Get(pa.Id.Value).Date.Should().Be.EqualTo(AgentDayConverters.DateOfUnconvertedSchedule);
			UnitOfWork.Clear();

			new PersonAssignmentDateSetter().ExecuteConverterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.Utc);

			paRep.Get(pa.Id.Value).Date.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldNotTouchAssignmentIfNotRestoreDate()
		{
			var paRep = new PersonAssignmentRepository(UnitOfWork);
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expected = new DateOnly(2000, 1, 1);

			var pa = createAndStoreAssignment(start);

			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			new PersonAssignmentDateSetter().ExecuteConverterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.Utc);

			paRep.Get(pa.Id.Value).Date.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldNotConvertWrongPerson()
		{
			var paRep = new PersonAssignmentRepository(UnitOfWork);
			var start = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);

			var pa = createAndStoreAssignment(start);
			Session.ResetDateForAllAssignmentsAndAudits();

			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			new PersonAssignmentDateSetter().ExecuteConverterAndWrapInTransaction(Guid.NewGuid(), TimeZoneInfo.Utc);

			paRep.Get(pa.Id.Value).Date.Should().Be.EqualTo(AgentDayConverters.DateOfUnconvertedSchedule);
		}

		[Test]
		public void ShouldSetCorrectDayWhenTimeZoneIsCrazy()
		{
			var paRep = new PersonAssignmentRepository(UnitOfWork);
			var start = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			var pa = createAndStoreAssignment(start);
			Session.ResetDateForAllAssignmentsAndAudits();

			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			new PersonAssignmentDateSetter().ExecuteConverterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));

			paRep.Get(pa.Id.Value).Date.Should().Be.EqualTo(new DateOnly(1999, 12, 31));
		}

		[Test]
		public void ShouldIncreaseVersionNumber()
		{
			var pa = createAndStoreAssignment(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			Session.ResetDateForAllAssignmentsAndAudits();
			UnitOfWork.PersistAll();
			UnitOfWork.Clear();

			var versionBefore = pa.Version;
			new PersonAssignmentDateSetter().ExecuteConverterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.Utc);
			new PersonAssignmentRepository(UnitOfWork).Get(pa.Id.Value).Version
			                                          .Should().Be.EqualTo(versionBefore + 1);
		}

		private IPersonAssignment createAndStoreAssignment(DateTime start)
		{
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("sdf"),
			                                                               SetupFixtureForAssembly.loggedOnPerson,
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