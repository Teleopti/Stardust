using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
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
		public FakeUserCulture UserCulture;
		public Areas.Global.FakePermissionProvider PermissionProvider;

		[Test]
		public void TargetShouldNotBeNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnCorrectDayScheduleSummaryForWorkingDay()
		{
			UserCulture.Is(CultureInfoFactory.CreateSwedishCulture());
			var scheduleDate = new DateOnly(2020,1,1);
			var person = PersonFactory.CreatePerson("Sherlock","Holmes");
			person.WithId();
			PeopleSearchProvider.Add(person);
			PersonRepository.Has(person);
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

			var result = Target.CreateWeekScheduleViewModel(searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(person.Id.GetValueOrDefault());
			first.DaySchedules.Count().Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019, 12, 30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020, 1, 5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020, 1, 1));
			first.DaySchedules[2].Title.Should().Be("Day");
			first.DaySchedules[2].Color.Should().Be("rgb(0,0,255)");
			first.DaySchedules[2].TimeSpan.Should()
				.Be(new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0)));    
		}

		[Test]
		public void ShouldReturnCorrectDayScheduleSummaryForDayOff()
		{
			UserCulture.Is(CultureInfoFactory.CreateSwedishCulture());
			var scheduleDate = new DateOnly(2020,1,1);
			var person = PersonFactory.CreatePerson("Sherlock","Holmes");
			person.WithId();
			PeopleSearchProvider.Add(person);
			PersonRepository.Has(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, scheduleDate, DayOffFactory.CreateDayOff(new Description("DayOff")));

			ScheduleStorage.Add(pa);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(searchTerm, scheduleDate, 20, 1);

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
			UserCulture.Is(CultureInfoFactory.CreateSwedishCulture());
			var scheduleDate = new DateOnly(2020,1,1);
			var person = PersonFactory.CreatePerson("Sherlock","Holmes");
			person.WithId();
			PeopleSearchProvider.Add(person);
			PersonRepository.Has(person);
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

			var result = Target.CreateWeekScheduleViewModel(searchTerm, scheduleDate, 20, 1);

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
			UserCulture.Is(CultureInfoFactory.CreateSwedishCulture());
			var scheduleDate = new DateOnly(2020,1,1);
			var person = PersonFactory.CreatePerson("Sherlock","Holmes");
			person.TerminatePerson(new DateOnly(2019, 12,1), new PersonAccountUpdaterDummy());
			person.WithId();
			PeopleSearchProvider.Add(person);
			PersonRepository.Has(person);
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

			var result = Target.CreateWeekScheduleViewModel(searchTerm, scheduleDate, 20, 1);

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
		public void ShouldShowPersonScheduleOnTheTerminationDate()
		{
			UserCulture.Is(CultureInfoFactory.CreateSwedishCulture());
			var scheduleDate = new DateOnly(2020,1,1);
			var person = PersonFactory.CreatePerson("Sherlock","Holmes");
			person.TerminatePerson(new DateOnly(2020,1,1),new PersonAccountUpdaterDummy());
			person.WithId();
			PeopleSearchProvider.Add(person);
			PersonRepository.Has(person);
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

			var result = Target.CreateWeekScheduleViewModel(searchTerm,scheduleDate,20,1);

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
			UserCulture.Is(CultureInfoFactory.CreateSwedishCulture());
			var scheduleDate = new DateOnly(2020,1,1);
			var person = PersonFactory.CreatePerson("Sherlock","Holmes");
			person.WithId();
			PeopleSearchProvider.Add(person);
			PersonRepository.Has(person);


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

			var result = Target.CreateWeekScheduleViewModel(searchTerm, scheduleDate, 20, 1);

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
			UserCulture.Is(CultureInfoFactory.CreateSwedishCulture());
			var scheduleDate = new DateOnly(2020,1,1);
			var person = PersonFactory.CreatePerson("Sherlock","Holmes");
			person.WithId();
			PeopleSearchProvider.Add(person);
			PeopleSearchProvider.AddPersonWithViewConfidentialPermission(person);
			PersonRepository.Has(person);


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

			var result = Target.CreateWeekScheduleViewModel(searchTerm, scheduleDate, 20, 1);

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
			UserCulture.Is(CultureInfoFactory.CreateSwedishCulture());
			var scheduleDate = new DateOnly(2020,1,1);
			var person = PersonFactory.CreatePerson("Sherlock","Holmes");
			person.WithId();
			person.WorkflowControlSet = new WorkflowControlSet();
			person.WorkflowControlSet.SchedulePublishedToDate = new DateTime(2019,1,1);
			PeopleSearchProvider.Add(person);
			PersonRepository.Has(person);

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

			var result = Target.CreateWeekScheduleViewModel(searchTerm, scheduleDate, 20, 1);

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
			UserCulture.Is(CultureInfoFactory.CreateSwedishCulture());
			var scheduleDate = new DateOnly(2020,1,1);
			var person = PersonFactory.CreatePerson("Sherlock","Holmes");
			person.WithId();
			person.WorkflowControlSet = new WorkflowControlSet();
			person.WorkflowControlSet.SchedulePublishedToDate = new DateTime(2019,1,1);
			PeopleSearchProvider.Add(person);
			PersonRepository.Has(person);

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

			var result = Target.CreateWeekScheduleViewModel(searchTerm, scheduleDate, 20, 1);

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
	}
}
