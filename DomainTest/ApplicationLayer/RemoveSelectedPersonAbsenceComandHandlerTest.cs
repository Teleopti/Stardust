using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	[TestFixture]
	public class RemoveSelectedPersonAbsenceComandHandlerTest : IIsolateSystem
	{
		public IScheduleStorage ScheduleStorage;
		public FakePersonRepository PersonRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public RemoveSelectedPersonAbsenceCommandHandler Target;

		[Test]
		public void ShouldRemoveSelectedPersonAbsenceFromShiftDay()
		{
			var scenario = ScenarioRepository.Has("Default");

			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions().WithId();
			PersonRepository.Has(person);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var periodForSelectedAbsence = new DateTimePeriod(2016, 10, 1, 10, 2016, 10, 1, 11);
			var periodForNotSelectedAbsence = new DateTimePeriod(2016, 10, 1, 14, 2016, 10, 1, 15);
			var date = new DateOnly(2016, 10, 1);

			var selectedPersonAbsence = new PersonAbsence(person, scenario,
				new AbsenceLayer(new Absence(), periodForSelectedAbsence)).WithId();
			var notSelectedPersonAbsence = new PersonAbsence(person, scenario,
				new AbsenceLayer(new Absence(), periodForNotSelectedAbsence)).WithId();
			PersonAbsenceRepository.Has(selectedPersonAbsence);
			PersonAbsenceRepository.Has(notSelectedPersonAbsence);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				person,
				mainActivity, new DateTimePeriod(2016, 10, 1, 8, 2016, 10, 1, 17));
			
			PersonAssignmentRepository.Add(personAssignment);

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), date.ToDateOnlyPeriod(), scenario);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				Person = person,
				PersonAbsenceId = selectedPersonAbsence.Id.Value,
				Date = date,
				ScheduleRange = scheduleDictionary?[person]
			};
			
			Target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);
			
			var allPersonAbsences = PersonAbsenceRepository.LoadAll();

			allPersonAbsences.Single().Period.Should().Be.EqualTo(new DateTimePeriod(2016, 10, 1, 14, 2016, 10, 1, 15));
		}
		
		[Test]
		public void ShouldRemoveSelectedPersonAbsenceFromEmptyDay()
		{
			var scenario = ScenarioRepository.Has("Default");

			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions().WithId();
			PersonRepository.Has(person);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var periodForSelectedAbsence = new DateTimePeriod(2016, 10, 1, 8, 2016, 10, 1, 17);
			var date = new DateOnly(2016, 10, 1);
			var selectedPersonAbsence = new PersonAbsence(person, scenario,
				new AbsenceLayer(new Absence(), periodForSelectedAbsence));

			selectedPersonAbsence.WithId();

			PersonAbsenceRepository.Has(selectedPersonAbsence);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), date.ToDateOnlyPeriod(), scenario);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				Person = person,
				PersonAbsenceId = selectedPersonAbsence.Id.Value,
				Date = date,
				ScheduleRange = scheduleDictionary?[person]
			};
			
			Target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);
			
			var allPersonAbsences = PersonAbsenceRepository.LoadAll();

			allPersonAbsences.Should().Be.Empty();
		}

		[Test]
		public void ShouldRemoveAbsenceThatCoversOvernightShift()
		{
			var scenario = ScenarioRepository.Has("Default");

			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions().WithId();
			PersonRepository.Has(person);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var periodForAbsence = new DateTimePeriod(2016, 10, 1, 0, 2016, 10, 4, 0);
			var date = new DateOnly(2016, 10, 2);
			var absenceLayer = new AbsenceLayer(new Absence(), periodForAbsence);
			var personAbsence = new PersonAbsence(person, scenario, absenceLayer).WithId();
			PersonAbsenceRepository.Has(personAbsence);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				person,
				mainActivity, new DateTimePeriod(2016, 10, 2, 22, 2016, 10, 3, 6));
			
			PersonAssignmentRepository.Add(personAssignment);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), date.ToDateOnlyPeriod(), scenario);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				Person = person,
				PersonAbsenceId = personAbsence.Id.Value,
				Date = date,
				ScheduleRange = scheduleDictionary?[person]
			};
			
			Target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);


			var allPersonAbsences = PersonAbsenceRepository.LoadAll();
			allPersonAbsences.Count().Should().Be.EqualTo(2);

			allPersonAbsences.First().Period.Should().Be.EqualTo(new DateTimePeriod(2016, 10, 1, 0, 2016, 10, 2, 0));
			allPersonAbsences.Last().Period.Should().Be.EqualTo(new DateTimePeriod(2016, 10, 3, 0, 2016, 10, 4, 0));
		}

		[Test]
		public void ShouldRemoveMultipleAbsencesWithinAShiftDay()
		{
			var scenario = ScenarioRepository.Has("Default");

			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions().WithId();
			PersonRepository.Has(person);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var periodForAbsence1 = new DateTimePeriod(2016, 10, 1, 10, 2016, 10, 1, 11);
			var periodForAbsence2 = new DateTimePeriod(2016, 10, 1, 14, 2016, 10, 1, 15);
			var date = new DateOnly(2016, 10, 1);

			var selectedPersonAbsence =
				new PersonAbsence(person, scenario, new AbsenceLayer(new Absence(), periodForAbsence1)).WithId();
			var selectedPersonAbsence2 =
				new PersonAbsence(person, scenario, new AbsenceLayer(new Absence(), periodForAbsence2)).WithId();
			PersonAbsenceRepository.Has(selectedPersonAbsence);
			PersonAbsenceRepository.Has(selectedPersonAbsence2);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				person,
				mainActivity, new DateTimePeriod(2016, 10, 1, 8, 2016, 10, 1, 17));

			PersonAssignmentRepository.Add(personAssignment);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), date.ToDateOnlyPeriod(), scenario);
			
			var command = new RemoveSelectedPersonAbsenceCommand
			{
				Person = person,
				PersonAbsenceId = selectedPersonAbsence.Id.Value,
				Date = date,
				ScheduleRange = scheduleDictionary[person]
			};

			Target.Handle(command);

			command.PersonAbsenceId = selectedPersonAbsence2.Id.Value;
			Target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);

			var allPersonAbsences = PersonAbsenceRepository.LoadAll();

			allPersonAbsences.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldRemoveByDayWithTimezoneAwarenessWhenRemovingOnMultidayAbsence()
		{
			var scenario = ScenarioRepository.Has("Default");

			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions().WithId();
			PersonRepository.Has(person);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var date = new DateOnly(2017, 2, 21);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
			var periodForAbsence = new DateTimePeriod(2017, 2, 20, 0, 2017, 2, 23, 8);

			var absenceLayer = new AbsenceLayer(new Absence(), periodForAbsence);
			var personAbsence = new PersonAbsence(person, scenario, absenceLayer).WithId();
			PersonAbsenceRepository.Has(personAbsence);

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				person,
				scenario, new DateTimePeriod(2017, 2, 21, 0, 2017, 2, 21, 8));

			PersonAssignmentRepository.Add(personAssignment);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), date.ToDateOnlyPeriod(), scenario);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				Person = person,
				PersonAbsenceId = personAbsence.Id.Value,
				Date = date,
				ScheduleRange = scheduleDictionary[person]
			};
			
			Target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);
			
			var allPersonAbsences = PersonAbsenceRepository.LoadAll();
			allPersonAbsences.Count().Should().Be.EqualTo(2);

			allPersonAbsences.First().Period.Should().Be.EqualTo(new DateTimePeriod(2017, 2, 20, 0, 2017, 2, 20, 15));
			allPersonAbsences.Last().Period.Should().Be.EqualTo(new DateTimePeriod(2017, 2, 21, 15, 2017, 2, 23, 8));
		}

		[Test]
		public void ShouldExcludeOvernightShiftPeriodWhenRemovingCurrentDateAbsenceButPreviousDayWithOvernightShift()
		{
			var scenario = ScenarioRepository.Has("Default");

			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions().WithId();
			PersonRepository.Has(person);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var date = new DateOnly(2018, 7, 24);

			var periodForAbsence = new DateTimePeriod(2018, 7, 23, 0, 2018, 7, 26, 0);
			var personAbsence = new PersonAbsence(person, scenario, new AbsenceLayer(new Absence(), periodForAbsence)).WithId();
			PersonAbsenceRepository.Has(personAbsence);

			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var shiftCatagory = ShiftCategoryFactory.CreateShiftCategory("DY");

			var personAssignmentForPreDay = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				person, scenario,
				 new DateTimePeriod(2018, 7, 23, 22, 2018, 7, 24, 6), shiftCatagory, mainActivity);

			PersonAssignmentRepository.Add(personAssignmentForPreDay);

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(new DateOnly(2018, 7, 23), date), scenario);

			var command = new RemoveSelectedPersonAbsenceCommand
			{
				Person = person,
				PersonAbsenceId = personAbsence.Id.Value,
				Date = date,
				ScheduleRange = scheduleDictionary?[person]
			};

			Target.Handle(command);

			var allPersonAbsences = PersonAbsenceRepository.LoadAll();
			allPersonAbsences.Count().Should().Be.EqualTo(2);

			allPersonAbsences.First().Period.Should().Be.EqualTo(new DateTimePeriod(2018, 7, 23, 0, 2018, 7, 24, 6));
			allPersonAbsences.Last().Period.Should().Be.EqualTo(new DateTimePeriod(2018, 7, 25, 0, 2018, 7, 26, 0));
		}

		
		[Test]
		public void ShouldRasieEventWithCorrectPeriodWhenRemovingAbsenceFromOverninght()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var scenario = ScenarioFactory.CreateScenario("default", true, false).WithId();
			scenario.SetBusinessUnit(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId());
			var abs = AbsenceFactory.CreateAbsence("abs").WithId();
			var shiftPeriod = new DateTimePeriod(2019, 1, 15, 20, 2019, 1, 16, 5);

			PersonRepository.Has(person);
			ScenarioRepository.Has(scenario);
			AbsenceRepository.Has(abs);

			var overNightShift =
				PersonAssignmentFactory.CreateAssignmentWithMainShiftAndOvertimeShift(person, scenario, shiftPeriod);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2019, 1, 16, 2, 2019, 1, 16, 3), abs).WithId();
			foreach (var shiftLayer in overNightShift.ShiftLayers)
			{
				shiftLayer.WithId();
			}
			PersonAssignmentRepository.Has(overNightShift);
			PersonAbsenceRepository.Has(personAbsence);

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(new DateOnly(2019, 1, 15), new DateOnly(2019, 1, 16)), scenario);

			Target.Handle(new RemoveSelectedPersonAbsenceCommand
			{
				Person = person,
				PersonAbsenceId = personAbsence.Id.Value,
				Date = new DateOnly(2019, 1, 15),
				ScheduleRange = scheduleDictionary[person]
			});

			personAbsence.PopAllEvents(null).Cast<PersonAbsenceRemovedEvent>().Single().StartDateTime.Should().Be.EqualTo(new DateTime(2019, 1, 15, 20, 0, 0));
		}


		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<RemoveSelectedPersonAbsenceCommandHandler>().For<RemoveSelectedPersonAbsenceCommandHandler>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}
	}
}
