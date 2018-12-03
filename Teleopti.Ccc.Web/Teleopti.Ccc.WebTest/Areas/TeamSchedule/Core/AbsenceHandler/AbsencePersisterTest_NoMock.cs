using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;


namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core.AbsenceHandler
{
	[TestFixture, TeamScheduleTest]
	public class AbsencePersisterTest_NoMock :IIsolateSystem
	{
		public IAbsencePersister Target;
		public FakePersonRepository PersonRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public IScheduleStorage ScheduleStorage;
		public FakeUserTimeZone UserTimeZone;
		public Global.FakePermissionProvider PermissionProvider;
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePersonRepository>().For<IProxyForId<IPerson>>();
			isolate.UseTestDouble<FakeAbsenceRepository>().For<IProxyForId<IAbsence>>();
			isolate.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			isolate.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			
		}

		[Test]
		public void ShouldReturnFailResultWhenNoPermissionOfViewUnpublishedScheduleForAddFullDayAbsence()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("Sherlock", "Homes");
			PersonRepository.Has(person);

			var date = new DateOnly(2018, 2, 12);
			var dateTimePeriod = new DateTimePeriod(2018, 2, 12, 8, 2018, 2, 12, 18);
			PermissionProvider.PublishToDate(date.AddDays(-1));
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence, person, date);
			
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit().WithId();
			scenario.SetBusinessUnit(businessUnit);
			ScenarioRepository.Has(scenario);

			var absence = AbsenceFactory.CreateAbsenceWithId();
			AbsenceRepository.Has(absence);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, dateTimePeriod, absence).WithId();
			ScheduleStorage.Add(personAbsence);

			var command = new AddFullDayAbsenceCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				StartDate = dateTimePeriod.StartDateTime,
				EndDate = dateTimePeriod.EndDateTime,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = person.Id.GetValueOrDefault(),
					TrackId = Guid.NewGuid()
				}
			};

			var result = Target.PersistFullDayAbsence(command);
			result.ErrorMessages.Single().Should().Be.EqualTo(Resources.NoPermissionToEditUnpublishedSchedule);
		}

		[Test]
		public void ShouldReturnFailResultWhenNoPermissionOfViewUnpublishedScheduleForAddIntrayDayAbsence()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("Sherlock", "Homes");
			PersonRepository.Has(person);

			var date = new DateOnly(2018, 2, 12);
			PermissionProvider.PublishToDate(date.AddDays(-1));
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence, person, date);

			var mainDateTimePeriod = new DateTimePeriod(2018, 2, 12, 8, 2018, 2, 12, 16);
			var intradayDateTimePeriod = new DateTimePeriod(2018, 2, 12, 8, 2018, 2, 12, 9);
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit().WithId();
			scenario.SetBusinessUnit(businessUnit);
			ScenarioRepository.Has(scenario);

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("main shift").WithId();
			var activity = ActivityFactory.CreateActivity("activity").WithId();
			
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, mainDateTimePeriod, shiftCategory, activity);

			var absence = AbsenceFactory.CreateAbsenceWithId();
			AbsenceRepository.Has(absence);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, intradayDateTimePeriod, absence).WithId();

			ScheduleStorage.Add(personAssignment);
			ScheduleStorage.Add(personAbsence);

			var command = new AddIntradayAbsenceCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				StartTime = intradayDateTimePeriod.StartDateTime,
				EndTime = intradayDateTimePeriod.EndDateTime,
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = person.Id.GetValueOrDefault(),
					TrackId = Guid.NewGuid()
				}
			};

			var result = Target.PersistIntradayAbsence(command);
			result.ErrorMessages.Single().Should().Be.EqualTo(Resources.NoPermissionToEditUnpublishedSchedule);
		}


	}
}
