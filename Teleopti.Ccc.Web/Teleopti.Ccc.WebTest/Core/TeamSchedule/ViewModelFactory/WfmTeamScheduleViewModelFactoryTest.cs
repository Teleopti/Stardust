using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Ccc.WebTest.Areas.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.ViewModelFactory
{
	[TestFixture, TeamScheduleTest]
	public class WfmTeamScheduleViewModelFactoryTest
	{
		public TeamScheduleViewModelFactory Target;
		public FakePeopleSearchProvider PeopleSearchProvider;
		public FakePersonRepository PersonRepository;
		public FakeScheduleStorage ScheduleStorage;
		public FakeCurrentScenario CurrentScenario;
		public FakeUserTimeZone UserTimeZone;
		public Areas.Global.FakePermissionProvider PermissionProvider;
		private ITeam team;
		private IPerson person;

		private void SetUpPersonAndCulture()
		{
			person = PersonFactory.CreatePerson("Sherlock","Holmes");
			person.WithId();

			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010,1,1),personContract,team);

			person.AddPersonPeriod(personPeriod);
			PeopleSearchProvider.Add(person);

			PersonRepository.Has(person);
		}

		[Test]
		public void TargetShouldNotBeNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnCorrectDayScheduleSummaryForWorkingDay()
		{
			SetUpPersonAndCulture();

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8,0,0, DateTimeKind.Utc),new DateTime(2020,1,1,17,0,0,DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));

			ScheduleStorage.Add(pa);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new []{team.Id.Value}, searchTerm, new DateOnly(2019, 12, 30), 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(person.Id.GetValueOrDefault());
			first.DaySchedules.Count().Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019, 12, 30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020, 1, 5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020, 1, 1));
			first.DaySchedules[2].Title.Should().Be("Day");
			first.DaySchedules[2].Color.Should().Be("rgb(0,0,255)");
			first.DaySchedules[2].DateTimeSpan.GetValueOrDefault().Should().Be(new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)));    
		}

		[Test]
		public void ShouldReturnCorrectDayScheduleSummaryForDayOff()
		{
			SetUpPersonAndCulture();
			var scheduleDate = new DateOnly(2020, 1, 1);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, scheduleDate, DayOffFactory.CreateDayOff(new Description("DayOff")));

			ScheduleStorage.Add(pa);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new []{team.Id.Value}, searchTerm, new DateOnly(2019, 12, 30), 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(person.Id.GetValueOrDefault());
			first.DaySchedules.Count().Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019, 12, 30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020, 1, 5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020, 1, 1));
			first.DaySchedules[2].Title.Should().Be("DayOff");
			first.DaySchedules[2].IsDayOff.Should().Be.True();
		}

		[Test]
		public void ShouldReturnCorrectDayScheduleSummaryForFullDayAbsence()
		{
			SetUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), AbsenceFactory.CreateAbsence("abs"));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new []{team.Id.Value}, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(person.Id.GetValueOrDefault());
			first.DaySchedules.Count.Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019, 12, 30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020, 1, 5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020, 1, 1));
			first.DaySchedules[2].Title.Should().Be("abs");
			first.DaySchedules[2].IsDayOff.Should().Be.False();
		}
		[Test]
		public void ShouldIndicateTerminationForTerminatedPerson()
		{
			SetUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			person.TerminatePerson(new DateOnly(2019,12,1),new PersonAccountUpdaterDummy());

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), AbsenceFactory.CreateAbsence("abs"));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new []{team.Id.Value}, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(person.Id.GetValueOrDefault());
			first.DaySchedules.Count.Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019, 12, 30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020, 1, 5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020, 1, 1));
			first.DaySchedules[2].Title.Should().Be(null);
			first.DaySchedules.All(d => d.IsTerminated).Should().Be.True();
		}
		
		[Test]
		public void ShouldListAgentWhoChangedToMatchingSearchDuringTheWeek()
		{
			SetUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			
			PeopleSearchProvider.EnableDateFilter();
			PeopleSearchProvider.Add(new DateOnly(2020, 1, 5), person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			CurrentScenario.FakeScenario(scenario);

			person.TerminatePerson(new DateOnly(2019,12,1),new PersonAccountUpdaterDummy());

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario,person,
				new DateTimePeriod(new DateTime(2020,1,1,8,0,0,DateTimeKind.Utc),new DateTime(2020,1,1,17,0,0,DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("Day","blue"));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person,scenario,
				new DateTimePeriod(new DateTime(2020,1,1,8,0,0,DateTimeKind.Utc),
					new DateTime(2020,1,1,17,0,0,DateTimeKind.Utc)),AbsenceFactory.CreateAbsence("abs"));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField,string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new []{team.Id.Value}, searchTerm,scheduleDate,20,1);

			result.Total.Should().Be(1);
			
			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(person.Id.GetValueOrDefault());
			first.DaySchedules.Count.Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019,12,30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020,1,5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020,1,1));
			first.DaySchedules[2].Title.Should().Be(null);
			first.DaySchedules.All(d => d.IsTerminated).Should().Be.True();
		}


		[Test]
		public void ShouldShowPersonScheduleOnTheTerminationDate()
		{
			SetUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario,person,
				new DateTimePeriod(new DateTime(2020,1,1,8,0,0,DateTimeKind.Utc),new DateTime(2020,1,1,17,0,0,DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("Day","blue"));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person,scenario,
				new DateTimePeriod(new DateTime(2020,1,1,8,0,0,DateTimeKind.Utc),
					new DateTime(2020,1,1,17,0,0,DateTimeKind.Utc)),AbsenceFactory.CreateAbsence("abs"));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField,string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new []{team.Id.Value}, searchTerm, new DateOnly(2019, 12, 30), 20,1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(person.Id.GetValueOrDefault());
			first.DaySchedules.Count.Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019,12,30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020,1,5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020,1,1));
			first.DaySchedules[2].Title.Should().Not.Be(null);
			first.DaySchedules[2].IsTerminated.Should().Be.False();		
		}


		[Test]
		public void ShouldReturnCorrectDayScheduleSummaryForNotPermittedConfidentialAbs()
		{
			SetUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var absence = AbsenceFactory.CreateAbsence("absence","abs", Color.Blue);
			absence.Confidential = true;
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), absence);

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new []{team.Id.Value}, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(person.Id.GetValueOrDefault());
			first.DaySchedules.Count.Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019, 12, 30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020, 1, 5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020, 1, 1));
			first.DaySchedules[2].Title.Should().Be(ConfidentialPayloadValues.Description.Name);
			first.DaySchedules[2].Color.Should().Be(ConfidentialPayloadValues.DisplayColorHex);
			first.DaySchedules[2].IsDayOff.Should().Be.False();
		}
		[Test]
		public void ShouldReturnCorrectDayScheduleSummaryForPermittedConfidentialAbs()
		{
			SetUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			PeopleSearchProvider.AddPersonWithViewConfidentialPermission(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var absence = AbsenceFactory.CreateAbsence("absence","abs", Color.Blue);
			absence.Confidential = true;
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), absence);

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new []{team.Id.Value}, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(person.Id.GetValueOrDefault());
			first.DaySchedules.Count.Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019, 12, 30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020, 1, 5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020, 1, 1));
			first.DaySchedules[2].Title.Should().Be("absence");
			first.DaySchedules[2].Color.Should().Be("rgb(0,0,255)");
			first.DaySchedules[2].IsDayOff.Should().Be.False();
		}
		[Test]
		public void ShouldReturnCorrectDayScheduleSummaryForNotPermittedUnpublishedSchedule()
		{
			SetUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			person.WorkflowControlSet = new WorkflowControlSet();
			person.WorkflowControlSet.SchedulePublishedToDate = new DateTime(2019,1,1);

			PermissionProvider.Enable();

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), AbsenceFactory.CreateAbsence("abs"));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new []{team.Id.Value}, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(person.Id.GetValueOrDefault());
			first.DaySchedules.Count.Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019, 12, 30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020, 1, 5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020, 1, 1));
			first.DaySchedules[2].Title.Should().Be(null);
			first.DaySchedules[2].IsDayOff.Should().Be.False();
		}
		[Test]
		public void ShouldReturnCorrectDayScheduleSummaryForPermittedUnpublishedSchedule()
		{
			SetUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			person.WorkflowControlSet = new WorkflowControlSet();
			person.WorkflowControlSet.SchedulePublishedToDate = new DateTime(2019,1,1);

			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), AbsenceFactory.CreateAbsence("abs"));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new []{team.Id.Value}, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(person.Id.GetValueOrDefault());
			first.DaySchedules.Count.Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019, 12, 30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020, 1, 5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020, 1, 1));
			first.DaySchedules[2].Title.Should().Be("abs");
			first.DaySchedules[2].IsDayOff.Should().Be.False();
		}

		[Test]
		public void ShouldProvideWeeklyContractTimeInWeeklyViewModel()
		{
			SetUpPersonAndCulture();
			var scheduleDate = new DateOnly(2020,1,1);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			CurrentScenario.FakeScenario(scenario);

			var activity = ActivityFactory.CreateActivity("Phone");
			activity.InContractTime = true;
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day", "blue");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity,person,new DateTimePeriod(new DateTime(2020,1,1,8,0,0,DateTimeKind.Utc),new DateTime(2020,1,1,17,0,0,DateTimeKind.Utc)),shiftCategory,scenario);
		
			ScheduleStorage.Add(pa);

			var searchTerm = new Dictionary<PersonFinderField,string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new []{team.Id.Value}, searchTerm,scheduleDate,20,1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(person.Id.GetValueOrDefault());			
			first.ContractTimeMinutes.Should().Be.EqualTo(9*60);
		}

		[Test]
		public void ShouldViewAgentOrderByLastNameInWeekView()
		{
			var scheduleDate = new DateOnly(2020,1,1);
			var person = PersonFactory.CreatePerson("Bill","Mamer");
			var person2 = PersonFactory.CreatePerson("Sherlock","Holmes");
			person.WithId();
			person2.WithId();
			PeopleSearchProvider.Add(person);
			PeopleSearchProvider.Add(person2);
			PersonRepository.Has(person);
			PersonRepository.Has(person2);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			CurrentScenario.FakeScenario(scenario);

			var activity = ActivityFactory.CreateActivity("Phone");
			activity.InContractTime = true;
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day","blue");

			var startTimeUtc = new DateTime(2020,1,1,0,0,0,DateTimeKind.Utc);
			var endTimeUtc = new DateTime(2020,1,1,9,0,0,DateTimeKind.Utc);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity,person,new DateTimePeriod(startTimeUtc,endTimeUtc),shiftCategory,scenario);
			var pa2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity,person2,new DateTimePeriod(startTimeUtc,endTimeUtc),shiftCategory,scenario);

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(pa2);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.All, "e"}
			};

			var result = Target.CreateWeekScheduleViewModel(new []{team.Id.Value}, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(2);

			result.PersonWeekSchedules.First().PersonId.Should().Be(person2.Id.GetValueOrDefault());
			result.PersonWeekSchedules.First().Name.Should().Be("Sherlock@Holmes");
			result.PersonWeekSchedules.Last().PersonId.Should().Be(person.Id.GetValueOrDefault());
			result.PersonWeekSchedules.Last().Name.Should().Be("Bill@Mamer");
		}

		[Test]
		public void CreateViewModelForPeopleShouldReturnAgentsEvenTheyhaveNoScheduleDay()
		{

			SetUpPersonAndCulture();
			var scheduleDate = new DateOnly(2020,1,1);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			CurrentScenario.FakeScenario(scenario);

			var result = Target.CreateViewModelForPeople(new[] {person.Id.Value}, scheduleDate);

			result.Total.Should().Be.EqualTo(1);
			var schedule = result.Schedules.Single();
			schedule.PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault().ToString());
			schedule.Projection.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnCorrectMultiplicatorDefinitionSetsInDayViewModel()
		{
			var scheduleDate = new DateOnly(2020,1,1);
			var person = PersonFactory.CreatePerson("Sherlock","Holmes");
			person.WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			CurrentScenario.FakeScenario(scenario);

			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("mds", MultiplicatorType.Overtime).WithId();
			contract.AddMultiplicatorDefinitionSetCollection(mds);

			ITeam team = TeamFactory.CreateSimpleTeam();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010,1,1),personContract,team);

			person.AddPersonPeriod(personPeriod);
			PeopleSearchProvider.Add(person);
			PersonRepository.Has(person);

			var result = Target.CreateViewModelForPeople(new[] { person.Id.Value },scheduleDate);

			result.Total.Should().Be.EqualTo(1);
			var schedule = result.Schedules.Single();
			schedule.PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault().ToString());
			schedule.Projection.Should().Be.Empty();
			schedule.MultiplicatorDefinitionSetIds.Single().Should().Be.EqualTo(mds.Id.Value);
		}
	}
}
