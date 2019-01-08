using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
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
	public class AbsencePersisterTest :IIsolateSystem
	{
		public IAbsencePersister Target;
		public FakePersonRepository PersonRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public IScheduleStorage ScheduleStorage;
		public FakeUserTimeZone UserTimeZone;
		public Global.FakePermissionProvider PermissionProvider;

		public FakePersonAssignmentRepository PersonAssignmentRepo;
		public FakePersonRepository PersonRepo;
		public FakeScenarioRepository ScenarioRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakeEventPublisher EventPublisher;
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			isolate.UseTestDouble<FakeAbsenceRepository>().For<IProxyForId<IAbsence>>();
			isolate.UseTestDouble<FakePersonRepository>().For<IProxyForId<IPerson>>();
			isolate.UseTestDouble<AbsencePersister>().For<IAbsencePersister>();
			isolate.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			isolate.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			isolate.UseTestDouble<FakeMeetingRepository>().For<IMeetingRepository>();
			isolate.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();

		}


		[Test]
		public void ShouldPersistFullDayAbsence()
		{
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit().WithId();
			scenario.SetBusinessUnit(businessUnit);

			var person = PersonFactory.CreatePerson().WithId();
			var abs = AbsenceFactory.CreateAbsence("abs").WithId();

			PersonRepo.Has(person);
			ScenarioRepository.Has(scenario);
			AbsenceRepository.Has(abs);

			var command = new AddFullDayAbsenceCommand {
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = abs.Id.GetValueOrDefault(),
				StartDate = new DateTime(2019, 1, 8, 0, 0, 0),
				EndDate = new DateTime(2019, 1, 9, 0, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = person.Id.GetValueOrDefault(),
					TrackId = Guid.NewGuid()
				}
			};
			var actionResult = Target.PersistFullDayAbsence(command);
			actionResult.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnErrorWhenNoPermissonForAddFullDayAbsence()
		{
			PermissionProvider.Enable();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit().WithId();
			scenario.SetBusinessUnit(businessUnit);

			var person = PersonFactory.CreatePerson().WithId();
			var abs = AbsenceFactory.CreateAbsence("abs").WithId();

			PersonRepo.Has(person);
			ScenarioRepository.Has(scenario);
			AbsenceRepository.Has(abs);

			var command = new AddFullDayAbsenceCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = abs.Id.GetValueOrDefault(),
				StartDate = new DateTime(2019, 1, 8, 0, 0, 0),
				EndDate = new DateTime(2019, 1, 9, 0, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = person.Id.GetValueOrDefault(),
					TrackId = Guid.NewGuid()
				}
			};
			var actionResult = Target.PersistFullDayAbsence(command);
			actionResult.ErrorMessages.Single().Should().Be.EqualTo(Resources.NoPermissionAddFullDayAbsenceForAgent);
		}

		[Test]
		public void ShouldPersistIntradayAbsence()
		{
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit().WithId();
			scenario.SetBusinessUnit(businessUnit);

			var person = PersonFactory.CreatePerson().WithId();
			var abs = AbsenceFactory.CreateAbsence("abs").WithId();

			PersonRepo.Has(person);
			ScenarioRepository.Has(scenario);
			AbsenceRepository.Has(abs);

			var command = new AddIntradayAbsenceCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = abs.Id.GetValueOrDefault(),
				StartTime = new DateTime(2019, 1, 8, 0, 0, 0),
				EndTime= new DateTime(2019, 1, 9, 0, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = person.Id.GetValueOrDefault(),
					TrackId = Guid.NewGuid()
				}
			};
			var actionResult = Target.PersistIntradayAbsence(command);
			actionResult.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnErrorWhenNoPermissonForAddIntradayAbsence()
		{
			PermissionProvider.Enable();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit().WithId();
			scenario.SetBusinessUnit(businessUnit);

			var person = PersonFactory.CreatePerson().WithId();
			var abs = AbsenceFactory.CreateAbsence("abs").WithId();

			PersonRepo.Has(person);
			ScenarioRepository.Has(scenario);
			AbsenceRepository.Has(abs);

			var command = new AddIntradayAbsenceCommand
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = abs.Id.GetValueOrDefault(),
				StartTime = new DateTime(2019, 1, 8, 0, 0, 0),
				EndTime = new DateTime(2019, 1, 9, 0, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					OperatedPersonId = person.Id.GetValueOrDefault(),
					TrackId = Guid.NewGuid()
				}
			};
			var actionResult = Target.PersistIntradayAbsence(command);
			actionResult.ErrorMessages.Single().Should().Be.EqualTo(Resources.NoPermisionAddIntradayAbsenceForAgent);
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

		[Test]
		public void ShouldRaiseEventWithPeriodIncludingScheduleDateWhenCurrentDayHasOvernightShiftAndTheIntradayAbsIsAddedToTheOvernightPart()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenario("default", true, false).WithId();
			scenario.SetBusinessUnit(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId());
			var abs = AbsenceFactory.CreateAbsence("abs").WithId();
			var shiftPeriod = new DateTimePeriod(2019, 1, 3, 20, 2019, 1, 4, 5);

			PersonRepository.Has(person);
			ScenarioRepository.Has(scenario);
			AbsenceRepository.Has(abs);

			var overNightShift =
				PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(person, scenario, shiftPeriod);
			foreach (var shiftLayer in overNightShift.ShiftLayers)
			{
				shiftLayer.WithId();
			}
			PersonAssignmentRepo.Has(overNightShift);

			Target.PersistIntradayAbsence(new AddIntradayAbsenceCommand
			{
				AbsenceId = abs.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				StartTime = new DateTime(2019, 1, 4, 4, 0, 0),
				EndTime = new DateTime(2019, 1, 4, 5, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			});


			EventPublisher.PublishedEvents.OfType<PersonAbsenceAddedEvent>().Single().StartDateTime.Should().Be.EqualTo(new DateTime(2019, 1, 3, 20, 0, 0));
		}
		
		[Test]
		public void ShouldRaiseEventWithPeriodIncludingScheduleDateWhenAddIntradayAbsence()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenario("default", true, false).WithId();
			scenario.SetBusinessUnit(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId());
			var abs = AbsenceFactory.CreateAbsence("abs").WithId();

			PersonRepo.Has(person);
			ScenarioRepository.Has(scenario);
			AbsenceRepository.Has(abs);

			Target.PersistIntradayAbsence(new AddIntradayAbsenceCommand
			{
				AbsenceId = abs.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				StartTime = new DateTime(2019, 1, 4, 4, 0, 0),
				EndTime = new DateTime(2019, 1, 4, 5, 0, 0),
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			});

			EventPublisher.PublishedEvents.OfType<PersonAbsenceAddedEvent>().Single().StartDateTime.Should().Be.EqualTo(new DateTime(2019, 1, 4, 4, 0, 0));
		}
	}
}
