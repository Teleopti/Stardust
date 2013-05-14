using System;
using System.Linq;
using NHibernate.Envers;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Repositories.Audit;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	public class PersonTimeZoneSetterForAuditTest : AuditTest
	{
		private IPersonAssignment pa;

		[Test]
		public void ShouldSetAuditAssignmentForPersonToDefaultDate()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndStoreAssignment(uow, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
				uow.PersistAll();
			}

			var revisions = Session.Auditer().GetRevisions(typeof(PersonAssignment), pa.Id.Value);
			new PersonTimeZoneSetter().ExecuteConverterAndWrapInTransaction(pa.Person.Id.Value, TimeZoneInfo.Local);
			Session.Auditer().Find<PersonAssignment>(pa.Id.Value, revisions.Last()).Date.Should().Be.EqualTo(AgentDayConverters.DateOfUnconvertedSchedule);
		}

		[Test]
		public void ShouldNotSetAuditAssignmentForWrongPersonToDefaultDate()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				createAndStoreAssignment(uow, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
				uow.PersistAll();
			}

			var revisions = Session.Auditer().GetRevisions(typeof(PersonAssignment), pa.Id.Value);
			new PersonTimeZoneSetter().ExecuteConverterAndWrapInTransaction(Guid.NewGuid(), TimeZoneInfo.Local);
			Session.Auditer().Find<PersonAssignment>(pa.Id.Value, revisions.Last()).Date.Should().Not.Be.EqualTo(AgentDayConverters.DateOfUnconvertedSchedule);
		}


		private void createAndStoreAssignment(IUnitOfWork uow, DateTime start)
		{
			pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("sdf"),
																																		 SetupFixtureForAssembly.loggedOnPerson,
																																		 new DateTimePeriod(start, start.AddHours(8)),
																																		 new ShiftCategory("d"), new Scenario("d"));
			var rep = new Repository(uow);
			rep.Add(pa.MainShift.LayerCollection[0].Payload);
			rep.Add(pa.ShiftCategory);
			pa.Scenario.DefaultScenario = true;
			rep.Add(pa.Scenario);
			rep.Add(pa);
		}
	}
}