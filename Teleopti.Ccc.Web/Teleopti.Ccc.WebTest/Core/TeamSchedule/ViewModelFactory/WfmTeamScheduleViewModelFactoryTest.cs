using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
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
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;
		private ITeam team;
		private IPerson personInUtc;
		private IPerson personInHongKong;


		private void setUpPersonAndCulture(bool addSecondPerson = false)
		{
			personInUtc = PersonFactory.CreatePerson("Sherlock", "Holmes");
			personInHongKong = PersonFactory.CreatePerson("A", "Detective");
			personInUtc.WithId();
			personInHongKong.WithId();
			personInHongKong.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);

			personInUtc.AddPersonPeriod(personPeriod);
			personInHongKong.AddPersonPeriod(personPeriod);
			PeopleSearchProvider.Add(personInUtc);
			if (addSecondPerson)
			{
				PeopleSearchProvider.Add(personInHongKong);
				PersonRepository.Has(personInHongKong);
			}
			PersonRepository.Has(personInUtc);

		}

		[Test]
		public void TargetShouldNotBeNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldSortScheduleInDifferentTimezoneByAbsoluteStartTime()
		{
			setUpPersonAndCulture(true);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));

			var pa2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInHongKong,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 4, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 13, 0, 0, DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));


			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(pa2);

			var searchTerm = new Dictionary<PersonFinderField, string>();

			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(2020, 1, 1),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});
			result.Total.Should().Be(2);
			result.Schedules.First().Name.Should().Be.EqualTo("A@Detective");
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsNoScheduleForScheduleSearch()
		{
			setUpPersonAndCulture();

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(2019, 12, 30),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});

			result.Total.Should().Be(1);

			var projectionVm = result.Schedules.Single().Projection;
			projectionVm.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsMainShiftForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			setUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 9, 0, 0, DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("Day", "blue"), ActivityFactory.CreateActivity("activity1", new Color()));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));

			ScheduleStorage.Add(pa);
			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});

			result.Schedules.Count().Should().Be.EqualTo(3);

			var schedule = result.Schedules.First();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(2);

			projectionVm[0].Description.Should().Be.EqualTo("activity1");
			projectionVm[0].Start.Should().Be.EqualTo("2020-01-01 08:00");
			projectionVm[0].Minutes.Should().Be.EqualTo(60);

			projectionVm[1].Description.Should().Be.EqualTo("activity2");
			projectionVm[1].Start.Should().Be.EqualTo("2020-01-01 09:00");
			projectionVm[1].Minutes.Should().Be.EqualTo(120);
		}

		[Test]
		public void ShouldReturnShiftCategoryDescriptionWhenThereIsMainShiftForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			setUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("testShift");
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 9, 0, 0, DateTimeKind.Utc)),
				shiftCategory, ActivityFactory.CreateActivity("activity1", new Color()));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));

			ScheduleStorage.Add(pa);
			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});

			var schedule = result.Schedules.First();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			schedule.ShiftCategory.Name.Should().Be.EqualTo(shiftCategory.Description.Name);
		}

		[Test]
		public void ShouldReturnNullShiftCategoryWhenThereIsDayOffForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			setUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("testShift");
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 9, 0, 0, DateTimeKind.Utc)),
				shiftCategory, ActivityFactory.CreateActivity("activity1", new Color()));
			pa.SetDayOff(new DayOffTemplate(new Description("DayOff")));

			ScheduleStorage.Add(pa);
			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});

			result.Schedules.Count().Should().Be.EqualTo(3);

			var schedule = result.Schedules.First();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			schedule.ShiftCategory.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullShiftCategoryWhenThereIsFullDayAbsenceOnlyForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			setUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("testShift");
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)),
				shiftCategory, ActivityFactory.CreateActivity("activity1", new Color()));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), AbsenceFactory.CreateAbsence("abs"));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);
			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});
			var schedule = result.Schedules.First();
			schedule.IsFullDayAbsence.Should().Be.EqualTo(true);
			schedule.ShiftCategory.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsDayOffForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			setUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("testShift");
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 9, 0, 0, DateTimeKind.Utc)),
				shiftCategory, ActivityFactory.CreateActivity("activity1", new Color()));
			pa.SetDayOff(new DayOffTemplate(new Description("DayOff")));

			ScheduleStorage.Add(pa);
			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});


			var schedule = result.Schedules.First();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(0);

			schedule.DayOff.DayOffName.Should().Be.EqualTo("DayOff");
			schedule.DayOff.Start.Should().Be.EqualTo("2020-01-01 00:00");
			schedule.DayOff.Minutes.Should().Be.EqualTo(1440);
		}

		[Test]
		public void ShouldGetCorrectIdsForNeighboringPersonAbsences()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			setUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("testShift");
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)),
				shiftCategory, ActivityFactory.CreateActivity("activity1", new Color()));


			var absence = AbsenceFactory.CreateAbsenceWithId();
			var personAbsence1 = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(2020, 1, 1, 11, 2020, 1, 1, 12), absence).WithId();
			var personAbsence2 = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(2020, 1, 1, 12, 2020, 1, 1, 13), absence).WithId();
			ScheduleStorage.Add(personAbsence1);
			ScheduleStorage.Add(personAbsence2);
			ScheduleStorage.Add(pa);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});

			var schedule = result.Schedules.First();
			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(3);
			projectionVm[0].ParentPersonAbsences.Should().Be.Null();
			projectionVm[1].ParentPersonAbsences.ToList().Contains(personAbsence1.Id.Value).Should().Be.EqualTo(true);
			projectionVm[1].ParentPersonAbsences.ToList().Contains(personAbsence2.Id.Value).Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsFullDayAbsenceOnlyForScheduleSearch()
		{

			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			setUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(new SearchDaySchedulesInput
			{
				TeamIds = new[] { team.Id.Value },
				CriteriaDictionary = searchTerm,
				DateInUserTimeZone = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			});

			var schedule = result.Schedules.First();
			schedule.IsFullDayAbsence.Should().Be.EqualTo(true);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(1);
			projectionVm.Single().Description.Should().Be.EqualTo(personAbsence.Layer.Payload.Description.Name);
			projectionVm.Single().Start.Should().Be.EqualTo("2020-01-01 08:00");
			projectionVm.Single().Minutes.Should().Be.EqualTo(480);
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsFullDayAbsenceAndShiftForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			setUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);


			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));
			var pa = PersonAssignmentFactory.CreatePersonAssignment(personInUtc, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 15));
			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});


			var schedule = result.Schedules.First();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(true);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(1);
			projectionVm.Single().Description.Should().Be.EqualTo(personAbsence.Layer.Payload.Description.Name);
			projectionVm.Single().Start.Should().Be.EqualTo("2020-01-01 09:00");
			projectionVm.Single().Minutes.Should().Be.EqualTo(360);
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsFullDayAbsenceAndDayoffForScheduleSearch()
		{

			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			setUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));
			var pa = PersonAssignmentFactory.CreateAssignmentWithDayOff(personInUtc, scenario,
				new DateOnly(scheduleDate), new DayOffTemplate(new Description("testDayoff")));
			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);


			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});



			var schedule = result.Schedules.First();
			schedule.IsFullDayAbsence.Should().Be.EqualTo(true);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(1);
			projectionVm.Single().Description.Should().Be.EqualTo(personAbsence.Layer.Payload.Description.Name);
			projectionVm.Single().Start.Should().Be.EqualTo("2020-01-01 08:00");
			projectionVm.Single().Minutes.Should().Be.EqualTo(480);
		}


		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsFullDayAbsenceOnAContractDayOffDayForScheduleSearch()
		{

			var scheduleDate = new DateTime(2020, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			team = TeamFactory.CreateSimpleTeam().WithId();
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(scheduleDate.AddDays(-1)), team).WithId();
			person.SetName(new Name("Sherlock", "Holmes"));
			var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(scheduleDate.AddDays(-1)));
			person.AddSchedulePeriod(schedulePeriod);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PeopleSearchProvider.Add(person);


			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(2020, 1, 4, 8, 2020, 1, 4, 17));
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2020, 1, 4));
			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);


			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});



			var schedule = result.Schedules.First();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(true);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(1);
			projectionVm.Single().Description.Should().Be.EqualTo(personAbsence.Layer.Payload.Description.Name);
			projectionVm.Single().Start.Should().Be.EqualTo("2020-01-04 08:00");
			projectionVm.Single().Minutes.Should().Be.EqualTo(480);
		}


		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsConfidentialAbsenceAndShiftAndNoPermissionForConfidentialForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			team = TeamFactory.CreateSimpleTeam().WithId();
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(scheduleDate.AddDays(-1)), team).WithId();
			person.SetName(new Name("Sherlock", "Holmes"));
			PeopleSearchProvider.Add(person);


			var secretAbs = AbsenceFactory.CreateAbsence("secret");
			secretAbs.Confidential = true;

			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", Color.White),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 15));
			ScheduleStorage.Add(pa);

			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 10), secretAbs);
			ScheduleStorage.Add(personAbsence);


			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});


			var schedule = result.Schedules.First();
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(2);

			projectionVm[0].Description.Should().Be.EqualTo(ConfidentialPayloadValues.Description.Name);
			projectionVm[0].Color.Should().Be.EqualTo(ConfidentialPayloadValues.DisplayColorHex);
			projectionVm[0].Start.Should().Be.EqualTo("2020-01-01 09:00");
			projectionVm[0].Minutes.Should().Be.EqualTo(60);

			projectionVm[1].Description.Should().Be.EqualTo("activity1");
			projectionVm[1].Color.Should().Be.EqualTo(Color.White.ToHtml());
			projectionVm[1].Start.Should().Be.EqualTo("2020-01-01 10:00");
			projectionVm[1].Minutes.Should().Be.EqualTo(300);
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsConfidentialAbsenceAndShiftAndWithPermissionForConfidentialForScheduleSearch()
		{
			setUpPersonAndCulture();
			PeopleSearchProvider.AddPersonWithViewConfidentialPermission(personInUtc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);


			var pa = PersonAssignmentFactory.CreatePersonAssignment(personInUtc, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", Color.White),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 15));
			ScheduleStorage.Add(pa);

			var secretAbs = AbsenceFactory.CreateAbsence("secret");
			secretAbs.Confidential = true;
			secretAbs.DisplayColor = Color.Red;
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 10), secretAbs);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});



			var schedule = result.Schedules.First();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");

			var projectionVm = schedule.Projection.ToList();
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			projectionVm.Count.Should().Be.EqualTo(2);

			projectionVm[0].Description.Should().Be.EqualTo("secret");
			projectionVm[0].Color.Should().Be.EqualTo(secretAbs.DisplayColor.ToHtml());
			projectionVm[0].Start.Should().Be.EqualTo("2020-01-01 09:00");
			projectionVm[0].Minutes.Should().Be.EqualTo(60);

			projectionVm[1].Description.Should().Be.EqualTo("activity1");
			projectionVm[1].Color.Should().Be.EqualTo(Color.White.ToHtml());
			projectionVm[1].Start.Should().Be.EqualTo("2020-01-01 10:00");
			projectionVm[1].Minutes.Should().Be.EqualTo(300);
		}


		[Test]
		public void ShouldReturnEmptyScheduleWhenScheduleIsUnpublishedAndNoViewUnpublishedSchedulePermissionForScheduleSearch()
		{
			setUpPersonAndCulture();

			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			personInUtc.WorkflowControlSet = new WorkflowControlSet("testWCS")
			{
				SchedulePublishedToDate = new DateTime(2019, 12, 30)
			};
			PeopleSearchProvider.Add(personInUtc);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);

			var pa = PersonAssignmentFactory.CreatePersonAssignment(personInUtc, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));

			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			ScheduleStorage.Add(pa);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});


			var schedule = result.Schedules.First();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");


			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnScheduleForPreviousDayWhenThereIsOvernightShiftForScheduleSearch()
		{
			setUpPersonAndCulture();

			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreatePersonAssignment(personInUtc, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			ScheduleStorage.Add(pa);

			var paPrev = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario, new DateTimePeriod(2019, 12, 31, 20, 2020, 1, 1, 3));
			ScheduleStorage.Add(paPrev);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});

			var schedules = result.Schedules;

			schedules.Count().Should().Be.EqualTo(3);
			schedules.First().Date.Should().Be.EqualTo("2020-01-01");
			schedules.Second().Date.Should().Be.EqualTo("2019-12-31");
		}

		[Test]
		public void ShouldReturnEmptyScheduleVmForEmptyScheduleWhenThereIsOvernightShiftForScheduleSearch()
		{
			setUpPersonAndCulture();
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var paPrev = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario, new DateTimePeriod(2019, 12, 31, 20, 2020, 1, 1, 3));
			ScheduleStorage.Add(paPrev);
			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});

			var schedules = result.Schedules;

			schedules.Count().Should().Be.EqualTo(3);
			schedules.First().Date.Should().Be.EqualTo("2020-01-01");
			schedules.First().Projection.Count().Should().Be.EqualTo(0);
			schedules.Second().Projection.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnEmptyScheduleVmWhenThereIsNoSelectedTeam()
		{
			setUpPersonAndCulture();
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var paPrev = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario, new DateTimePeriod(2019, 12, 31, 20, 2020, 1, 1, 3));
			ScheduleStorage.Add(paPrev);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new Guid[0],
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});

			var schedules = result.Schedules;

			schedules.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldIndicateOvertimeActivityForScheduleSearch()
		{
			setUpPersonAndCulture();
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(personInUtc, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddOvertimeActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11),
				new MultiplicatorDefinitionSet("temp", MultiplicatorType.Overtime));
			ScheduleStorage.Add(pa);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});

			result.Schedules.Count().Should().Be.EqualTo(3);
			result.Schedules.First().Date.Should().Be.EqualTo("2020-01-01");
			result.Schedules.First().Projection.First().IsOvertime.Should().Be.EqualTo(false);
			result.Schedules.First().Projection.Last().IsOvertime.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnSchedulesWithCorrectOrderForScheduleSearch()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("a1", "a1");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("b1", "b1");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("c1", "c1");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);
			var person4 = PersonFactory.CreatePersonWithGuid("d1", "d1");
			PeopleSearchProvider.Add(person4);
			person4.AddPersonPeriod(personPeriod);
			var person5 = PersonFactory.CreatePersonWithGuid("e1", "e1");
			PeopleSearchProvider.Add(person5);
			person5.AddPersonPeriod(personPeriod);
			var person6 = PersonFactory.CreatePersonWithGuid("f1", "f1");
			PeopleSearchProvider.Add(person6);
			person6.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa1);

			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 7, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa2);

			var pa3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));
			ScheduleStorage.Add(pa3);

			var personAbsenceForPerson4 = PersonAbsenceFactory.CreatePersonAbsence(person4, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));

			var pa4 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person4,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa4);
			ScheduleStorage.Add(personAbsenceForPerson4);

			var scheduleDay5 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person5, scenario);
			scheduleDay5.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			var dayOffAssigment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person5, scenario,
				new DateOnly(scheduleDate), new DayOffTemplate(new Description("day off")));
			ScheduleStorage.Add(dayOffAssigment);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "a1 b1 c1 d1 e1 f1"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});

			var schedules = result.Schedules.ToArray();

			result.Total.Should().Be.EqualTo(6);
			schedules[0].Name.Should().Be.EqualTo("b1@b1");
			schedules[3].Name.Should().Be.EqualTo("a1@a1");
			schedules[6].Name.Should().Be.EqualTo("c1@c1");
			schedules[9].Name.Should().Be.EqualTo("d1@d1");
			schedules[12].Name.Should().Be.EqualTo("e1@e1");
			schedules[15].Name.Should().Be.EqualTo("f1@f1");
		}

		[Test]
		public void ShouldSetUniqueShiftLayerIdForLayersBelongingToOneShiftLayer()
		{
			setUpPersonAndCulture();
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(personInUtc, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 10, 2020, 1, 1, 14));
			pa.ShiftLayers.ForEach(l => l.WithId());

			ScheduleStorage.Add(pa);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});

			var shiftLayers = pa.ShiftLayers.ToList();
			var schedule = result.Schedules.First();
			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(3);
			projectionVm[0].ShiftLayerIds.First().Should().Be.EqualTo(shiftLayers[0].Id);
			projectionVm[1].ShiftLayerIds.First().Should().Be.EqualTo(shiftLayers[1].Id);
			projectionVm[2].ShiftLayerIds.First().Should().Be.EqualTo(shiftLayers[0].Id);
		}

		[Test]
		public void ShouldReturnSchedulesWithCorrectOrderOnContractDayOffDayForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc);
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(scheduleDate.AddDays(-1)), team);
			personPeriod.PersonContract.ContractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());

			var person1 = PersonFactory.CreatePersonWithGuid("a1", "a1");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);

			var person2 = PersonFactory.CreatePersonWithGuid("b1", "b1");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);

			var person3 = PersonFactory.CreatePersonWithGuid("c1", "c1");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);

			var person4 = PersonFactory.CreatePersonWithGuid("d1", "d1");
			PeopleSearchProvider.Add(person4);
			person4.AddPersonPeriod(personPeriod);

			var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(scheduleDate.AddDays(-1)));
			person4.AddSchedulePeriod(schedulePeriod);

			var person5 = PersonFactory.CreatePersonWithGuid("e1", "e1");
			PeopleSearchProvider.Add(person5);
			person5.AddPersonPeriod(personPeriod);

			var person6 = PersonFactory.CreatePersonWithGuid("f1", "f1");
			PeopleSearchProvider.Add(person6);
			person6.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 4, 8, 2020, 1, 4, 9), ShiftCategoryFactory.CreateShiftCategory("test"));
			ScheduleStorage.Add(pa1);

			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 4, 7, 2020, 1, 4, 9), ShiftCategoryFactory.CreateShiftCategory("test"));
			ScheduleStorage.Add(pa2);

			var pa3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 4, 8, 2020, 1, 4, 9), ShiftCategoryFactory.CreateShiftCategory("test"));
			ScheduleStorage.Add(pa3);

			var personAbsenceForPerson4 = PersonAbsenceFactory.CreatePersonAbsence(person4, scenario,
				new DateTimePeriod(2020, 1, 4, 8, 2020, 1, 4, 17));
			var pa4 = PersonAssignmentFactory.CreatePersonAssignment(person4, scenario, new DateOnly(2020, 1, 4));
			ScheduleStorage.Add(pa4);
			ScheduleStorage.Add(personAbsenceForPerson4);

			var scheduleDay5 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person5, scenario);
			scheduleDay5.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			var dayOffAssigment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person5, scenario,
				new DateOnly(scheduleDate), new DayOffTemplate(new Description("day off")));
			ScheduleStorage.Add(dayOffAssigment);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "a1 b1 c1 d1 e1 f1"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});
			var schedules = result.Schedules.ToArray();

			result.Total.Should().Be.EqualTo(6);
			schedules[0].Name.Should().Be.EqualTo("b1@b1");
			schedules[3].Name.Should().Be.EqualTo("a1@a1");
			schedules[6].Name.Should().Be.EqualTo("c1@c1");
			schedules[9].Name.Should().Be.EqualTo("d1@d1");
			schedules[12].Name.Should().Be.EqualTo("e1@e1");
			schedules[15].Name.Should().Be.EqualTo("f1@f1");
		}

		[Test]
		public void ShouldReturnSchedulesWithCorrectOrderForSpecificPageForScheduleSearch()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("a1", "a1");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("b1", "b1");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("c1", "c1");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);
			var person4 = PersonFactory.CreatePersonWithGuid("d1", "d1");
			PeopleSearchProvider.Add(person4);
			person4.AddPersonPeriod(personPeriod);
			var person5 = PersonFactory.CreatePersonWithGuid("e1", "e1");
			PeopleSearchProvider.Add(person5);
			person5.AddPersonPeriod(personPeriod);
			var person6 = PersonFactory.CreatePersonWithGuid("f1", "f1");
			PeopleSearchProvider.Add(person6);
			person6.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa1);

			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 7, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa2);

			var pa3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));
			ScheduleStorage.Add(pa3);

			var personAbsenceForPerson4 = PersonAbsenceFactory.CreatePersonAbsence(person4, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));

			var pa4 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person4,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa4);
			ScheduleStorage.Add(personAbsenceForPerson4);

			var scheduleDay5 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person5, scenario);
			scheduleDay5.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			var dayOffAssigment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person5, scenario,
				new DateOnly(scheduleDate), new DayOffTemplate(new Description("day off")));
			ScheduleStorage.Add(dayOffAssigment);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "a1 b1 c1 d1 e1 f1"}
			};
			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 2,
					CurrentPageIndex = 2,
					IsOnlyAbsences = false
				});

			var schedules = result.Schedules.ToArray();
			result.Total.Should().Be.EqualTo(6);

			schedules[0].Name.Should().Be.EqualTo("c1@c1");
			schedules[1].Name.Should().Be.EqualTo("c1@c1");
			schedules[3].Name.Should().Be.EqualTo("d1@d1");
		}

		[Test]
		public void ShouldReturnCorrectProjectionForPeople()
		{
			setUpPersonAndCulture();
			PersonRepository.Add(personInHongKong);
			var scheduleDate = new DateTime(2015, 01, 01, 00, 00, 00, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var personAssignment1 = new PersonAssignment(personInUtc, scenario, scheduleDateOnly);
			personAssignment1.AddActivity(new Activity("activity1"), new DateTimePeriod(scheduleDate.AddHours(7), scheduleDate.AddHours(18)));
			ScheduleStorage.Add(personAssignment1);

			var personAssignment2 = new PersonAssignment(personInHongKong, scenario, scheduleDateOnly);
			personAssignment2.AddActivity(new Activity("activity2"), new DateTimePeriod(scheduleDate.AddHours(8), scheduleDate.AddHours(18)));
			ScheduleStorage.Add(personAssignment2);

			var result = Target.CreateViewModelForPeople(
				new[] { personInUtc.Id.Value, personInHongKong.Id.Value }, new DateOnly(scheduleDate));

			result.Total.Should().Be(2);
			var schedules = result.Schedules.ToArray();
			schedules.First().PersonId.Should().Be(personInUtc.Id.Value.ToString());
			schedules[3].PersonId.Should().Be(personInHongKong.Id.Value.ToString());
			schedules.First().Projection.First().Description.Should().Be("activity1");
			schedules[3].Projection.First().Description.Should().Be("activity2");
		}

		[Test]
		public void ShouldReturnCorrectDayScheduleSummaryForWorkingDay()
		{
			setUpPersonAndCulture();

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));

			ScheduleStorage.Add(pa);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new[] { team.Id.Value }, searchTerm, new DateOnly(2019, 12, 30), 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(personInUtc.Id.GetValueOrDefault());
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
			setUpPersonAndCulture();
			var scheduleDate = new DateOnly(2020, 1, 1);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithDayOff(personInUtc, scenario, scheduleDate, DayOffFactory.CreateDayOff(new Description("DayOff")));

			ScheduleStorage.Add(pa);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new[] { team.Id.Value }, searchTerm, new DateOnly(2019, 12, 30), 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(personInUtc.Id.GetValueOrDefault());
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
			setUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), AbsenceFactory.CreateAbsence("abs"));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new[] { team.Id.Value }, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(personInUtc.Id.GetValueOrDefault());
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
			setUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			personInUtc.TerminatePerson(new DateOnly(2019, 12, 1), new PersonAccountUpdaterDummy());

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), AbsenceFactory.CreateAbsence("abs"));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new[] { team.Id.Value }, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(personInUtc.Id.GetValueOrDefault());
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
			setUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), AbsenceFactory.CreateAbsence("abs"));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new[] { team.Id.Value }, searchTerm, new DateOnly(2019, 12, 30), 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(personInUtc.Id.GetValueOrDefault());
			first.DaySchedules.Count.Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019, 12, 30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020, 1, 5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020, 1, 1));
			first.DaySchedules[2].Title.Should().Not.Be(null);
			first.DaySchedules[2].IsTerminated.Should().Be.False();
		}

		[Test]
		public void ShouldReturnCorrectDayScheduleSummaryForNotPermittedConfidentialAbs()
		{
			setUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var absence = AbsenceFactory.CreateAbsence("absence", "abs", Color.Blue);
			absence.Confidential = true;
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), absence);

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new[] { team.Id.Value }, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(personInUtc.Id.GetValueOrDefault());
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
			setUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			PeopleSearchProvider.AddPersonWithViewConfidentialPermission(personInUtc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var absence = AbsenceFactory.CreateAbsence("absence", "abs", Color.Blue);
			absence.Confidential = true;
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), absence);

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new[] { team.Id.Value }, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(personInUtc.Id.GetValueOrDefault());
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
			setUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			personInUtc.WorkflowControlSet = new WorkflowControlSet();
			personInUtc.WorkflowControlSet.SchedulePublishedToDate = new DateTime(2019, 1, 1);

			PermissionProvider.Enable();

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), AbsenceFactory.CreateAbsence("abs"));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new[] { team.Id.Value }, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(personInUtc.Id.GetValueOrDefault());
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
			setUpPersonAndCulture();
			var scheduleDate = new DateOnly(2019, 12, 30);
			personInUtc.WorkflowControlSet = new WorkflowControlSet();
			personInUtc.WorkflowControlSet.SchedulePublishedToDate = new DateTime(2019, 1, 1);

			PermissionProvider.Enable();
			PermissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), ShiftCategoryFactory.CreateShiftCategory("Day", "blue"));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(personInUtc, scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), AbsenceFactory.CreateAbsence("abs"));

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(personAbsence);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new[] { team.Id.Value }, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(personInUtc.Id.GetValueOrDefault());
			first.DaySchedules.Count.Should().Be(7);
			first.DaySchedules[0].Date.Should().Be(new DateOnly(2019, 12, 30));
			first.DaySchedules[6].Date.Should().Be(new DateOnly(2020, 1, 5));
			first.DaySchedules[2].Date.Should().Be(new DateOnly(2020, 1, 1));
			first.DaySchedules[2].Title.Should().Be("abs");
			first.DaySchedules[2].IsDayOff.Should().Be.False();
		}

		[Test]
		public void ShouldReturnInternalNotesWhenThereIsForScheduleSearch()
		{
			setUpPersonAndCulture();
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			var note = new Note(personInUtc, new DateOnly(scheduleDate), scenario, "dummy notes");
			ScheduleStorage.Add(note);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});
			var schedule = result.Schedules.First();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.InternalNotes.Should().Be("dummy notes");
		}

		[Test]
		public void ShouldReturnTimezoneWhenThereIsForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			setUpPersonAndCulture();
			personInUtc.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});
			result.Total.Should().Be.EqualTo(1);

			var schedule = result.Schedules.First();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			new[] {
				"(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi",
				"(UTC+08:00) Beijing, Chongqing, Hong Kong SAR, Urumqi" // This is changed on new OS version, both values are correct.
			}.Should().Contain(schedule.Timezone.DisplayName);
			schedule.Timezone.IanaId.Should().Be("Asia/Shanghai");
		}

		[Test]
		public void ShouldProvideWeeklyContractTimeInWeeklyViewModel()
		{
			setUpPersonAndCulture();
			var scheduleDate = new DateOnly(2020, 1, 1);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var activity = ActivityFactory.CreateActivity("Phone");
			activity.InContractTime = true;
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day", "blue");

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc, scenario, activity, new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 17, 0, 0, DateTimeKind.Utc)), shiftCategory);

			ScheduleStorage.Add(pa);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateWeekScheduleViewModel(new[] { team.Id.Value }, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(1);

			var first = result.PersonWeekSchedules.First();

			first.PersonId.Should().Be(personInUtc.Id.GetValueOrDefault());
			first.ContractTimeMinutes.Should().Be.EqualTo(9 * 60);
		}

		[Test]
		public void ShouldViewAgentOrderByLastNameInWeekView()
		{
			var scheduleDate = new DateOnly(2020, 1, 1);
			team = TeamFactory.CreateTeamWithId("test");
			var person = PersonFactory.CreatePerson("Bill", "Mamer");
			var person2 = PersonFactory.CreatePerson("Sherlock", "Holmes");
			person.WithId();
			person2.WithId();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(scheduleDate.AddDays(-1), team));
			person2.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(scheduleDate.AddDays(-1), team));
			PeopleSearchProvider.Add(person);
			PeopleSearchProvider.Add(person2);
			PersonRepository.Has(person);
			PersonRepository.Has(person2);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var activity = ActivityFactory.CreateActivity("Phone");
			activity.InContractTime = true;
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day", "blue");

			var startTimeUtc = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var endTimeUtc = new DateTime(2020, 1, 1, 9, 0, 0, DateTimeKind.Utc);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(startTimeUtc, endTimeUtc), shiftCategory);
			var pa2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, activity, new DateTimePeriod(startTimeUtc, endTimeUtc), shiftCategory);

			ScheduleStorage.Add(pa);
			ScheduleStorage.Add(pa2);

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.All, "e"}
			};

			var result = Target.CreateWeekScheduleViewModel(new[] { team.Id.Value }, searchTerm, scheduleDate, 20, 1);

			result.Total.Should().Be(2);

			result.PersonWeekSchedules.First().PersonId.Should().Be(person2.Id.GetValueOrDefault());
			result.PersonWeekSchedules.First().Name.Should().Be("Sherlock@Holmes");
			result.PersonWeekSchedules.Last().PersonId.Should().Be(person.Id.GetValueOrDefault());
			result.PersonWeekSchedules.Last().Name.Should().Be("Bill@Mamer");
		}

		[Test]
		public void CreateViewModelForPeopleShouldReturnAgentsEvenTheyhaveNoScheduleDay()
		{

			setUpPersonAndCulture();
			var scheduleDate = new DateOnly(2020, 1, 1);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var result = Target.CreateViewModelForPeople(new[] { personInUtc.Id.Value }, scheduleDate);

			result.Total.Should().Be.EqualTo(1);
			var schedule = result.Schedules.Single();
			schedule.PersonId.Should().Be.EqualTo(personInUtc.Id.GetValueOrDefault().ToString());
			schedule.Projection.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnCorrectMultiplicatorDefinitionSetsInDayViewModel()
		{
			var scheduleDate = new DateOnly(2020, 1, 1);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			person.WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			var mds = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("mds", MultiplicatorType.Overtime).WithId();
			contract.AddMultiplicatorDefinitionSetCollection(mds);

			ITeam team = TeamFactory.CreateSimpleTeam();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);

			person.AddPersonPeriod(personPeriod);
			PeopleSearchProvider.Add(person);
			PersonRepository.Has(person);

			var result = Target.CreateViewModelForPeople(new[] { person.Id.Value }, scheduleDate);

			result.Total.Should().Be.EqualTo(1);
			var schedule = result.Schedules.Single();
			schedule.PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault().ToString());
			schedule.Projection.Should().Be.Empty();
			schedule.MultiplicatorDefinitionSetIds.Single().Should().Be.EqualTo(mds.Id.Value);
		}

		[Test]
		public void ShouldReturnCorrectOrderIfSortByFirstName()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("a1", "a1");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("f1", "f1");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("c1", "c1");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa1);

			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 7, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa2);

			var pa3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));
			ScheduleStorage.Add(pa3);


			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					SortOption = TeamScheduleSortOption.FirstName
				});

			var schedules = result.Schedules.ToArray();

			schedules[0].Name.Should().Be.EqualTo("a1@a1");
			schedules[3].Name.Should().Be.EqualTo("c1@c1");
			schedules[6].Name.Should().Be.EqualTo("f1@f1");
		}

		[Test]
		[SetUICulture("zh-CN")]
		public void ShouldBasedOnUserUiCultureIfSortByFirstName() {

			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("a1", "a1");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);

			var person2 = PersonFactory.CreatePersonWithGuid("绿", "俄");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);

			var person3 = PersonFactory.CreatePersonWithGuid("阿", "画");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);


			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					SortOption = TeamScheduleSortOption.FirstName
				});

			var schedules = result.Schedules.ToArray();

			schedules[0].Name.Should().Be.EqualTo("a1@a1");
			schedules[1].Name.Should().Be.EqualTo("阿@画");
			schedules[2].Name.Should().Be.EqualTo("绿@俄");
		}

		[Test]
		public void ShouldReturnCorrectOrderIfSortByLastName()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("a1", "e1");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("f1", "c1");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("c1", "k1");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa1);

			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 7, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa2);

			var pa3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));
			ScheduleStorage.Add(pa3);


			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					SortOption = TeamScheduleSortOption.LastName
				});

			var schedules = result.Schedules.ToArray();

			schedules[0].Name.Should().Be.EqualTo("f1@c1");
			schedules[3].Name.Should().Be.EqualTo("a1@e1");
			schedules[6].Name.Should().Be.EqualTo("c1@k1");
		}

		[Test]
		public void ShouldReturnCorrectOrderIfSortByEmploymentNumber()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("a1", "e1");
			person1.SetEmploymentNumber("456");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("f1", "c1");
			person2.SetEmploymentNumber("123");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("c1", "k1");
			person3.SetEmploymentNumber("254");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa1);

			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 7, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa2);

			var pa3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));
			ScheduleStorage.Add(pa3);


			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					SortOption = TeamScheduleSortOption.EmploymentNumber
				});

			var schedules = result.Schedules.ToArray();

			schedules[0].Name.Should().Be.EqualTo("f1@c1");
			schedules[3].Name.Should().Be.EqualTo("c1@k1");
			schedules[6].Name.Should().Be.EqualTo("a1@e1");
		}
		[Test]
		public void ShouldReturnCorrectOrderIfSortByStartTime()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("a1", "e1");
			person1.SetEmploymentNumber("456");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("f1", "c1");
			person2.SetEmploymentNumber("123");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("c1", "k1");
			person3.SetEmploymentNumber("254");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa1);

			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 7, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa2);

			var pa3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));
			ScheduleStorage.Add(pa3);


			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					SortOption = TeamScheduleSortOption.StartTime
				});

			var schedules = result.Schedules.ToArray();

			schedules[0].Name.Should().Be.EqualTo("f1@c1");
			schedules[6].Name.Should().Be.EqualTo("c1@k1");
			schedules[3].Name.Should().Be.EqualTo("a1@e1");
		}
		[Test]
		public void ShouldReturnCorrectOrderIfSortByEndTime()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("p1", "e1");
			person1.SetEmploymentNumber("456");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("p2", "c1");
			person2.SetEmploymentNumber("123");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("p3", "k1");
			person3.SetEmploymentNumber("254");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 18), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa1);

			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 7, 2020, 1, 1, 12), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa2);

			var pa3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"));
			ScheduleStorage.Add(pa3);


			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					SortOption = TeamScheduleSortOption.EndTime
				});

			var schedules = result.Schedules.ToArray();


			schedules[0].Name.Should().Be.EqualTo("p3@k1");
			schedules[3].Name.Should().Be.EqualTo("p2@c1");
			schedules[6].Name.Should().Be.EqualTo("p1@e1");
		}

		[Test]
		public void ShouldReturnCorrectOrderWhenSortingByStartTimeIfNoPermissionToViewUnpublishedSchedule()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var wfc = new WorkflowControlSet();
			wfc.SchedulePublishedToDate = new DateTime(2019, 12, 31);
			var person1 = PersonFactory.CreatePersonWithGuid("a1", "e1");
			person1.SetEmploymentNumber("456");
			person1.WorkflowControlSet = wfc;
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("f1", "c1");
			person2.SetEmploymentNumber("123");
			person2.WorkflowControlSet = wfc;
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("c1", "k1");
			person3.SetEmploymentNumber("254");
			person3.WorkflowControlSet = wfc;
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);
			PermissionProvider.Enable();
			PermissionProvider.PublishToDate(new DateOnly(new DateTime(2019, 12, 31)));

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa1);

			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 10, 2020, 1, 1, 17), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa2);

			var pa3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 6, 2020, 1, 1, 17), ShiftCategoryFactory.CreateShiftCategory("test"));
			ScheduleStorage.Add(pa3);


			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					SortOption = TeamScheduleSortOption.StartTime
				});

			var schedules = result.Schedules.ToArray();

			schedules[0].Name.Should().Be.EqualTo("f1@c1");
			schedules[3].Name.Should().Be.EqualTo("a1@e1");
			schedules[6].Name.Should().Be.EqualTo("c1@k1");
		}

		[Test]
		public void ShouldReturnCorrectOrderWhenSortingByStartTimeWithEmptySchedule()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("a1", "e1");
			person1.SetEmploymentNumber("456");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("f1", "c1");
			person2.SetEmploymentNumber("123");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("c1", "k1");
			person3.SetEmploymentNumber("254");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 10, 2020, 1, 1, 17), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa1);

			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa2);



			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					SortOption = TeamScheduleSortOption.StartTime
				});

			var schedules = result.Schedules.ToArray();

			schedules[0].Name.Should().Be.EqualTo("f1@c1");
			schedules[3].Name.Should().Be.EqualTo("a1@e1");
			schedules[6].Name.Should().Be.EqualTo("c1@k1");
		}

		[Test]
		public void ShouldReturnCorrectOrderWhenSortingByStartTimeWithEmptyScheduleAndFullDayAbsence()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("a1", "e1");
			person1.SetEmploymentNumber("456");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("f1", "c1");
			person2.SetEmploymentNumber("123");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("c1", "k1");
			person3.SetEmploymentNumber("254");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 10, 2020, 1, 1, 17), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa1);

			var personAbs = PersonAbsenceFactory.CreatePersonAbsence(person2, scenario,
				new DateTimePeriod(2020, 1, 1, 0, 2020, 1, 1, 23));

			ScheduleStorage.Add(personAbs);

			var pa3 = PersonAssignmentFactory.CreateEmptyAssignment(person3, scenario,
				new DateTimePeriod(2020, 1, 1, 0, 2020, 1, 1, 23));
			ScheduleStorage.Add(pa3);


			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					SortOption = TeamScheduleSortOption.StartTime
				});

			var schedules = result.Schedules.ToArray();

			schedules[0].Name.Should().Be.EqualTo("f1@c1");
			schedules[3].Name.Should().Be.EqualTo("a1@e1");
			schedules[6].Name.Should().Be.EqualTo("c1@k1");
		}
		[Test]
		public void ShouldReturnCorrectOrderWhenSortingByStartTimeWithEmptyScheduleAndDayOff()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("a1", "e1");
			person1.SetEmploymentNumber("456");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("f1", "c1");
			person2.SetEmploymentNumber("123");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("c1", "k1");
			person3.SetEmploymentNumber("254");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 10, 2020, 1, 1, 17), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa1);

			var pa2 = PersonAssignmentFactory.CreateEmptyAssignment(person2, scenario,
				new DateTimePeriod(2020, 1, 1, 0, 2020, 1, 1, 23));
			ScheduleStorage.Add(pa2);

			var pa3 = PersonAssignmentFactory.CreateAssignmentWithDayOff(person3, scenario, new DateOnly(2020, 1, 1),
				new DayOffTemplate());

			ScheduleStorage.Add(pa3);


			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					SortOption = TeamScheduleSortOption.StartTime
				});

			var schedules = result.Schedules.ToArray();
			schedules[0].Name.Should().Be.EqualTo("a1@e1");
			schedules[3].Name.Should().Be.EqualTo("c1@k1");
			schedules[6].Name.Should().Be.EqualTo("f1@c1");
		}
		[Test]
		public void ShouldReturnCorrectOrderWhenSortingByStartTimeWithDayOffSchedule()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("p1", "p1");
			person1.SetEmploymentNumber("456");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("p2", "p2");
			person2.SetEmploymentNumber("123");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("p3", "p3");
			person3.SetEmploymentNumber("254");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 10, 2020, 1, 1, 17), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa1);

			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1", new Color()), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17), ShiftCategoryFactory.CreateShiftCategory("test"));

			ScheduleStorage.Add(pa2);

			var pa3 = PersonAssignmentFactory.CreateAssignmentWithDayOff(person3, scenario,
				new DateOnly(scheduleDate), new DayOffTemplate(new Description("testDayoff")));
			ScheduleStorage.Add(pa3);

			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					SortOption = TeamScheduleSortOption.StartTime
				});

			var schedules = result.Schedules.ToArray();

			schedules[0].Name.Should().Be.EqualTo("p2@p2");
			schedules[3].Name.Should().Be.EqualTo("p1@p1");
			schedules[6].Name.Should().Be.EqualTo("p3@p3");
		}

		[Test]
		public void ShouldReturnCorrectOrderWhenSortingByStartTimeOnlyWithDayOffOrEmptySchedule()
		{
			var contract = ContractFactory.CreateContract("Contract");
			contract.WithId();
			team = TeamFactory.CreateSimpleTeam().WithId();
			IPersonContract personContract = PersonContractFactory.CreatePersonContract(contract);
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1), personContract, team);


			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("p1", "p1");
			PeopleSearchProvider.Add(person1);
			person1.AddPersonPeriod(personPeriod);
			var person2 = PersonFactory.CreatePersonWithGuid("p2", "p2");
			PeopleSearchProvider.Add(person2);
			person2.AddPersonPeriod(personPeriod);
			var person3 = PersonFactory.CreatePersonWithGuid("p3", "p3");
			PeopleSearchProvider.Add(person3);
			person3.AddPersonPeriod(personPeriod);
			var person4 = PersonFactory.CreatePersonWithGuid("p4", "p4");
			PeopleSearchProvider.Add(person4);
			person4.AddPersonPeriod(personPeriod);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var pa2 = PersonAssignmentFactory.CreateAssignmentWithDayOff(person2, scenario,
				new DateOnly(scheduleDate), new DayOffTemplate(new Description("testDayoff")));
			ScheduleStorage.Add(pa2);

			var pa3 = PersonAssignmentFactory.CreateAssignmentWithDayOff(person3, scenario,
				new DateOnly(scheduleDate), new DayOffTemplate(new Description("testDayoff")));
			ScheduleStorage.Add(pa3);

			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					SortOption = TeamScheduleSortOption.StartTime
				});

			var schedules = result.Schedules.ToArray();

			schedules[0].Name.Should().Be.EqualTo("p2@p2");
			schedules[3].Name.Should().Be.EqualTo("p3@p3");
			schedules[6].Name.Should().Be.EqualTo("p1@p1");
			schedules[7].Name.Should().Be.EqualTo("p4@p4");

		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsNoScheduleForScheduleSearchWithGroup()
		{
			var person1 = PersonFactory.CreatePersonWithGuid("a1", "a1");
			PeopleSearchProvider.Add(person1);
			var groupPage1 = new ReadOnlyGroupPage { PageId = Guid.NewGuid(), PageName = "group page 1" };
			var groupPageDetails1 = new ReadOnlyGroupDetail
			{
				PageId = groupPage1.PageId,
				GroupId = Guid.NewGuid(),
				PersonId = person1.Id.Value
			};

			GroupingReadOnlyRepository.Has(new List<ReadOnlyGroupPage>()
			{
				groupPage1
			}, new List<ReadOnlyGroupDetail>
			{
				groupPageDetails1
			});

			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.All, ""}
			};

			var result = Target.CreateViewModelForGroups(
				new SearchDaySchedulesInput
				{
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(2019, 12, 30),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					GroupIds = new Guid[] { groupPageDetails1.GroupId }
				});

			var schedules = result.Schedules.ToArray();

			result.Total.Should().Be.EqualTo(1);
			schedules[0].PersonId.Should().Be.EqualTo(person1.Id.ToString());
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsMainShiftForScheduleSearchWithGroup() {
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			setUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var groupPage1 = new ReadOnlyGroupPage { PageId = Guid.NewGuid(), PageName = "group page 1" };
			var groupPageDetails1 = new ReadOnlyGroupDetail
			{
				PageId = groupPage1.PageId,
				GroupId = Guid.NewGuid(),
				PersonId = personInUtc.Id.Value
			};

			GroupingReadOnlyRepository.Has(new List<ReadOnlyGroupPage>()
			{
				groupPage1
			}, new List<ReadOnlyGroupDetail>
			{

				groupPageDetails1
			});

			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 9, 0, 0, DateTimeKind.Utc)),
				ShiftCategoryFactory.CreateShiftCategory("Day", "blue"), ActivityFactory.CreateActivity("activity1", new Color()));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));

			ScheduleStorage.Add(pa);
			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateViewModelForGroups(
				new SearchDaySchedulesInput
				{
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false,
					GroupIds = new Guid[] { groupPageDetails1.GroupId }
				});
			result.Schedules.Count().Should().Be.EqualTo(3);

			var schedule = result.Schedules.First();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(2);

			projectionVm[0].Description.Should().Be.EqualTo("activity1");
			projectionVm[0].Start.Should().Be.EqualTo("2020-01-01 08:00");
			projectionVm[0].Minutes.Should().Be.EqualTo(60);

			projectionVm[1].Description.Should().Be.EqualTo("activity2");
			projectionVm[1].Start.Should().Be.EqualTo("2020-01-01 09:00");
			projectionVm[1].Minutes.Should().Be.EqualTo(120);
		}
		[Test]
		public void ShouldReturnShiftCategoryDescriptionWhenThereIsMainShiftForScheduleSearchWithGroup()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			setUpPersonAndCulture();
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);

			var groupPage1 = new ReadOnlyGroupPage { PageId = Guid.NewGuid(), PageName = "group page 1" };
			var groupPageDetails1 = new ReadOnlyGroupDetail
			{
				PageId = groupPage1.PageId,
				GroupId = Guid.NewGuid(),
				PersonId = personInUtc.Id.Value
			};

			GroupingReadOnlyRepository.Has(new List<ReadOnlyGroupPage>()
			{
				groupPage1
			}, new List<ReadOnlyGroupDetail>
			{

				groupPageDetails1
			});
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("testShift");
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personInUtc,
				scenario,
				new DateTimePeriod(new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2020, 1, 1, 9, 0, 0, DateTimeKind.Utc)),
				shiftCategory, ActivityFactory.CreateActivity("activity1", new Color()));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));

			ScheduleStorage.Add(pa);
			var searchTerm = new Dictionary<PersonFinderField, string>
			{
				{PersonFinderField.FirstName, "Sherlock"}
			};

			var result = Target.CreateViewModel(
				new SearchDaySchedulesInput
				{
					TeamIds = new[] { team.Id.Value },
					CriteriaDictionary = searchTerm,
					DateInUserTimeZone = new DateOnly(scheduleDate),
					PageSize = 20,
					CurrentPageIndex = 1,
					IsOnlyAbsences = false
				});

			var schedule = result.Schedules.First();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			schedule.ShiftCategory.Name.Should().Be.EqualTo(shiftCategory.Description.Name);
		}
	}
}
