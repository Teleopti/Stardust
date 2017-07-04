using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	[TestFixture, TeamScheduleTest, Ignore("temp ignore and fixing")]
	public class TeamScheduleControllerIntegrationTest:ISetup
	{
		public TeamScheduleController Target;
		public FakeSchedulePersonProvider PersonProvider;
		public FakeScheduleProvider ScheduleProvider;
		public Global.FakePermissionProvider PermissionProvider;
		public FakePeopleSearchProvider PeopleSearchProvider;
		public FakePersonRepository PersonRepository;
		public FakeToggleManager ToggleManager;

		[Test]
		public void TargetShouldNotBeNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsNoScheduleForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes"); // Refer to FakeCommonAgentNameProvider
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsMainShiftForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1",new Color()),
				new DateTimePeriod(2020,1,1,8,2020,1,1,9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2",new Color()),
				new DateTimePeriod(2020,1,1,9,2020,1,1,11));
			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
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
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("testShift");
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,scenario,new DateTimePeriod(2020,1,1,8,2020,1,1,9), shiftCategory);
			pa.AddActivity(ActivityFactory.CreateActivity("activity2",new Color()),
				new DateTimePeriod(2020,1,1,9,2020,1,1,11));
			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			schedule.ShiftCategory.Name.Should().Be.EqualTo(shiftCategory.Description.Name);
		}

		[Test]
		public void ShouldReturnNullShiftCategoryWhenThereIsDayOffForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(scheduleDate));
			scheduleDay.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1",new Color()),
				new DateTimePeriod(2020,1,1,9,2020,1,1,11));
			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			schedule.ShiftCategory.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullShiftCategoryWhenThereIsFullDayAbsenceOnlyForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var team = TeamFactory.CreateSimpleTeam().WithId();
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(scheduleDate.AddDays(-1)), team).WithId();
			person.SetName(new Name("Sherlock", "Holmes"));
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person,scenario,
				new DateTimePeriod(2020,1,1,8,2020,1,1,17));
			scheduleDay.Add(personAbsence);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(2020,1,1));

			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				SelectedTeamIds = new[] { team.Id.Value },
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
			schedule.IsFullDayAbsence.Should().Be.EqualTo(true);
			schedule.ShiftCategory.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsDayOffForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(scheduleDate));
			scheduleDay.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1",new Color()),
				new DateTimePeriod(2020,1,1,9,2020,1,1,11));
			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(0);

			schedule.DayOff.DayOffName.Should().Be.EqualTo("test");
			schedule.DayOff.Start.Should().Be.EqualTo("2020-01-01 00:00");
			schedule.DayOff.Minutes.Should().Be.EqualTo(1440);
		}

		[Test]
		public void ShouldGetCorrectIdsForNeighboringPersonAbsences()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1",new Color()),
				new DateTimePeriod(2020,1,1,8,2020,1,1,17));
			var absence = AbsenceFactory.CreateAbsenceWithId();
			var personAbsence1 = PersonAbsenceFactory.CreatePersonAbsence(person,scenario,
				new DateTimePeriod(2020,1,1,11,2020,1,1,12),absence).WithId();
			var personAbsence2 = PersonAbsenceFactory.CreatePersonAbsence(person,scenario,
				new DateTimePeriod(2020,1,1,12,2020,1,1,13),absence).WithId();
			scheduleDay.Add(personAbsence1);
			scheduleDay.Add(personAbsence2);

			scheduleDay.Add(pa);
			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();

			var schedule = result.Single();
			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(3);

			projectionVm[0].ParentPersonAbsences.Should().Be.Null();
			projectionVm[1].ParentPersonAbsences.ToList().Contains(personAbsence1.Id.Value).Should().Be.EqualTo(true);
			projectionVm[1].ParentPersonAbsences.ToList().Contains(personAbsence2.Id.Value).Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsFullDayAbsenceOnlyForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var team = TeamFactory.CreateSimpleTeam().WithId();
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(scheduleDate.AddDays(-1)), team).WithId();
			person.SetName(new Name("Sherlock", "Holmes"));
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person,scenario,
				new DateTimePeriod(2020,1,1,8,2020,1,1,17));
			scheduleDay.Add(personAbsence);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(2020,1,1));

			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				SelectedTeamIds = new[] { team.Id.Value },
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
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
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person,scenario,
				new DateTimePeriod(2020,1,1,8,2020,1,1,17));
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1",new Color()),
				new DateTimePeriod(2020,1,1,9,2020,1,1,15));
			scheduleDay.Add(pa);
			scheduleDay.Add(personAbsence);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
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
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var team = TeamFactory.CreateSimpleTeam().WithId();
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(scheduleDate.AddDays(-1)), team).WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person,scenario,
				new DateTimePeriod(2020,1,1,8,2020,1,1,17));
			var pa = PersonAssignmentFactory.CreateAssignmentWithDayOff(person,scenario,
				new DateOnly(scheduleDate), new DayOffTemplate(new Description("testDayoff")));
			scheduleDay.Add(pa);
			scheduleDay.Add(personAbsence);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				SelectedTeamIds = new []{team.Id.Value},
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
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
			var scheduleDate = new DateTime(2020,1,4,0,0,0,0,DateTimeKind.Utc);
			var team = TeamFactory.CreateSimpleTeam().WithId();
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(scheduleDate.AddDays(-1)), team).WithId();
			person.SetName(new Name("Sherlock", "Holmes"));
			var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(scheduleDate.AddDays(-1)));
			person.AddSchedulePeriod(schedulePeriod);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person,scenario,
				new DateTimePeriod(2020,1,4,8,2020,1,4,17));
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(2020,1,4));
			scheduleDay.Add(pa);
			scheduleDay.Add(personAbsence);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				SelectedTeamIds = new[] { team.Id.Value },
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(true);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(1);
			projectionVm.Single().Description.Should().Be.EqualTo(personAbsence.Layer.Payload.Description.Name);
			projectionVm.Single().Start.Should().Be.EqualTo("2020-01-04 08:00");
			projectionVm.Single().Minutes.Should().Be.EqualTo(480);
		}

		[Test]
		public void
			ShouldReturnCorrectProjectionWhenThereIsConfidentialAbsenceAndShiftAndNoPermissionForConfidentialForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("pNoConfidential","pNoConfidential");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var secretAbs = AbsenceFactory.CreateAbsence("secret");
			secretAbs.Confidential = true;
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1",Color.White),
				new DateTimePeriod(2020,1,1,9,2020,1,1,15));
			scheduleDay.Add(pa);
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(secretAbs,new DateTimePeriod(2020,1,1,9,2020,1,1,10)));

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
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
		public void
			ShouldReturnCorrectProjectionWhenThereIsConfidentialAbsenceAndShiftAndWithPermissionForConfidentialForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);
			PeopleSearchProvider.AddPersonWithViewConfidentialPermission(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var secretAbs = AbsenceFactory.CreateAbsence("secret");
			secretAbs.Confidential = true;
			secretAbs.DisplayColor = Color.Red;
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1",Color.White),
				new DateTimePeriod(2020,1,1,9,2020,1,1,15));
			scheduleDay.Add(pa);
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(secretAbs,new DateTimePeriod(2020,1,1,9,2020,1,1,10)));

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
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
			PermissionProvider.Enable();
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			person.WorkflowControlSet = new WorkflowControlSet("testWCS")
			{
				SchedulePublishedToDate = new DateTime(2019,12,30)
			};
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1",new Color()),
				new DateTimePeriod(2020,1,1,8,2020,1,1,9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2",new Color()),
				new DateTimePeriod(2020,1,1,9,2020,1,1,11));
			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnScheduleForPreviousDayWhenThereIsOvernightShiftForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1",new Color()),
				new DateTimePeriod(2020,1,1,8,2020,1,1,9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2",new Color()),
				new DateTimePeriod(2020,1,1,9,2020,1,1,11));
			scheduleDay.Add(pa);
			ScheduleProvider.AddScheduleDay(scheduleDay);

			var scheduleDayPrevious = ScheduleDayFactory.Create(new DateOnly(scheduleDate).AddDays(-1),person,scenario);
			var paPrev = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2019,12,31,20,2020,1,1,3));
			scheduleDayPrevious.Add(paPrev);
			ScheduleProvider.AddScheduleDay(scheduleDayPrevious);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();

			result.Count.Should().Be.EqualTo(2);
			result.First().Date.Should().Be.EqualTo("2020-01-01");
			result.Second().Date.Should().Be.EqualTo("2019-12-31");
		}
		[Test]
		public void ShouldReturnEmptyScheduleVmForEmptyScheduleWhenThereIsOvernightShiftForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);

			var scheduleDayPrevious = ScheduleDayFactory.Create(new DateOnly(scheduleDate).AddDays(-1),person,scenario);
			var paPrev = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2019,12,31,20,2020,1,1,3));
			scheduleDayPrevious.Add(paPrev);
			ScheduleProvider.AddScheduleDay(scheduleDayPrevious);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();

			result.Count.Should().Be.EqualTo(2);
			result.First().Date.Should().Be.EqualTo("2020-01-01");
			result.Second().Date.Should().Be.EqualTo("2019-12-31");
		}
		[Test]
		public void ShouldReturnEmptyScheduleVmWhenThereIsNoSelectedTeamAfterEnablingOrganizaationPicker()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);

			var scheduleDayPrevious = ScheduleDayFactory.Create(new DateOnly(scheduleDate).AddDays(-1),person,scenario);
			var paPrev = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2019,12,31,20,2020,1,1,3));
			scheduleDayPrevious.Add(paPrev);
			ScheduleProvider.AddScheduleDay(scheduleDayPrevious);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false,
				SelectedTeamIds = new Guid[0]
			}).Content.Schedules.ToList();

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnEmptyScheduleVmWhenThereIsNoSearchTermAndUserHasNoTeam()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);

			var scheduleDayPrevious = ScheduleDayFactory.Create(new DateOnly(scheduleDate).AddDays(-1),person,scenario);
			var paPrev = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2019,12,31,20,2020,1,1,3));
			scheduleDayPrevious.Add(paPrev);
			ScheduleProvider.AddScheduleDay(scheduleDayPrevious);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				SelectedTeamIds = new Guid[]{},
				Keyword = "",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldIndicateOvertimeActivityForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1",new Color()),
				new DateTimePeriod(2020,1,1,8,2020,1,1,9));
			pa.AddOvertimeActivity(ActivityFactory.CreateActivity("activity2",new Color()),
				new DateTimePeriod(2020,1,1,9,2020,1,1,11),
				new MultiplicatorDefinitionSet("temp",MultiplicatorType.Overtime));
			scheduleDay.Add(pa);
			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();

			result.Count.Should().Be.EqualTo(1);
			var schedule = result.First();
			schedule.Date.Should().Be.EqualTo("2020-01-01");
			schedule.Projection.First().IsOvertime.Should().Be.EqualTo(false);
			schedule.Projection.Last().IsOvertime.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnSchedulesWithCorrectOrderForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("a1","a1");
			PeopleSearchProvider.Add(person1);
			var person2 = PersonFactory.CreatePersonWithGuid("b1","b1");
			PeopleSearchProvider.Add(person2);
			var person3 = PersonFactory.CreatePersonWithGuid("c1","c1");
			PeopleSearchProvider.Add(person3);
			var person4 = PersonFactory.CreatePersonWithGuid("d1","d1");
			PeopleSearchProvider.Add(person4);
			var person5 = PersonFactory.CreatePersonWithGuid("e1","e1");
			PeopleSearchProvider.Add(person5);
			var person6 = PersonFactory.CreatePersonWithGuid("f1","f1");
			PeopleSearchProvider.Add(person6);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);

			var scheduleDay1 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person1,scenario);
			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1",new Color()), new DateTimePeriod(2020,1,1,8,2020,1,1,9), ShiftCategoryFactory.CreateShiftCategory("test"));

			scheduleDay1.Add(pa1);

			var scheduleDay2 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person2,scenario);
			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1",new Color()), new DateTimePeriod(2020,1,1,7,2020,1,1,9), ShiftCategoryFactory.CreateShiftCategory("test"));


			scheduleDay2.Add(pa2);

			var scheduleDay3 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person3,scenario);
			var pa3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
					scenario, ActivityFactory.CreateActivity("activity1",new Color()), new DateTimePeriod(2020,1,1,8,2020,1,1,9), ShiftCategoryFactory.CreateShiftCategory("test"));
			scheduleDay3.Add(pa3);


			var scheduleDay4 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person4,scenario);
			var personAbsenceForPerson4 = PersonAbsenceFactory.CreatePersonAbsence(person4,scenario,
				new DateTimePeriod(2020,1,1,8,2020,1,1,17));


			var pa4 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person4,
					scenario, ActivityFactory.CreateActivity("activity1",new Color()), new DateTimePeriod(2020,1,1,8,2020,1,1,9), ShiftCategoryFactory.CreateShiftCategory("test"));

			scheduleDay4.Add(pa4);
			scheduleDay4.Add(personAbsenceForPerson4);

			var scheduleDay5 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person5,scenario);
			scheduleDay5.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			ScheduleProvider.AddScheduleDay(scheduleDay4);
			ScheduleProvider.AddScheduleDay(scheduleDay5);
			ScheduleProvider.AddScheduleDay(scheduleDay3);
			ScheduleProvider.AddScheduleDay(scheduleDay1);
			ScheduleProvider.AddScheduleDay(scheduleDay2);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "firstName:a1 b1 c1 d1 e1 f1",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();

			result.Count.Should().Be.EqualTo(6);
			result[0].Name.Should().Be.EqualTo("b1@b1");
			result[1].Name.Should().Be.EqualTo("a1@a1");
			result[2].Name.Should().Be.EqualTo("c1@c1");
			result[3].Name.Should().Be.EqualTo("d1@d1");
			result[4].Name.Should().Be.EqualTo("e1@e1");
			result[5].Name.Should().Be.EqualTo("f1@f1");
		}
		[Test]
		public void ShouldSetUniqueShiftLayerIdForLayersBelongingToOneShiftLayer()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person,scenario,new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1",new Color()),
				new DateTimePeriod(2020,1,1,8,2020,1,1,17));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1",new Color()),
				new DateTimePeriod(2020,1,1,10,2020,1,1,14));
			pa.ShiftLayers.ForEach(l => l.WithId());

			scheduleDay.Add(pa);
			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			var shiftLayers = pa.ShiftLayers.ToList();
			var schedule = result.Single();
			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(3);
			projectionVm[0].ShiftLayerIds.First().Should().Be.EqualTo(shiftLayers[0].Id);
			projectionVm[1].ShiftLayerIds.First().Should().Be.EqualTo(shiftLayers[1].Id);
			projectionVm[2].ShiftLayerIds.First().Should().Be.EqualTo(shiftLayers[0].Id);
		}

		[Test]
		public void ShouldReturnSchedulesWithCorrectOrderOnContractDayOffDayForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,4,0,0,0,0,DateTimeKind.Utc);
			var team = TeamFactory.CreateSimpleTeam().WithId();
			var person1 = PersonFactory.CreatePersonWithGuid("a1","a1");
			PeopleSearchProvider.Add(person1);
			var person2 = PersonFactory.CreatePersonWithGuid("b1","b1");
			PeopleSearchProvider.Add(person2);
			var person3 = PersonFactory.CreatePersonWithGuid("c1","c1");
			PeopleSearchProvider.Add(person3);
			var person4 = PersonFactory.CreatePersonWithGuid("d1","d1");
			PeopleSearchProvider.Add(person4);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(scheduleDate.AddDays(-1)), team);
			personPeriod.PersonContract.ContractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());
			person4.AddPersonPeriod(personPeriod);
			var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(scheduleDate.AddDays(-1)));
			person4.AddSchedulePeriod(schedulePeriod);

			var person5 = PersonFactory.CreatePersonWithGuid("e1","e1");
			PeopleSearchProvider.Add(person5);
			var person6 = PersonFactory.CreatePersonWithGuid("f1","f1");
			PeopleSearchProvider.Add(person6);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);

			var scheduleDay1 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person1,scenario);
			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1",new Color()), new DateTimePeriod(2020,1,4,8,2020,1,4,9), ShiftCategoryFactory.CreateShiftCategory("test"));

			scheduleDay1.Add(pa1);

			var scheduleDay2 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person2,scenario);
			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1",new Color()), new DateTimePeriod(2020,1,4,7,2020,1,4,9), ShiftCategoryFactory.CreateShiftCategory("test"));


			scheduleDay2.Add(pa2);

			var scheduleDay3 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person3,scenario);
			var pa3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
					scenario, ActivityFactory.CreateActivity("activity1",new Color()), new DateTimePeriod(2020,1,4,8,2020,1,4,9), ShiftCategoryFactory.CreateShiftCategory("test"));
			scheduleDay3.Add(pa3);


			var scheduleDay4 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person4,scenario);
			var personAbsenceForPerson4 = PersonAbsenceFactory.CreatePersonAbsence(person4,scenario,
				new DateTimePeriod(2020,1,4,8,2020,1,4,17));
			var pa4 = PersonAssignmentFactory.CreatePersonAssignment(person4,scenario,new DateOnly(2020,1,4));
			scheduleDay4.Add(pa4);

			scheduleDay4.Add(personAbsenceForPerson4);

			var scheduleDay5 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person5,scenario);
			scheduleDay5.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			ScheduleProvider.AddScheduleDay(scheduleDay4);
			ScheduleProvider.AddScheduleDay(scheduleDay5);
			ScheduleProvider.AddScheduleDay(scheduleDay3);
			ScheduleProvider.AddScheduleDay(scheduleDay1);
			ScheduleProvider.AddScheduleDay(scheduleDay2);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				SelectedTeamIds = new[] { team.Id.Value },
				Keyword = "firstName:a1 b1 c1 d1 e1 f1",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();

			result.Count.Should().Be.EqualTo(6);
			result[0].Name.Should().Be.EqualTo("b1@b1");
			result[1].Name.Should().Be.EqualTo("a1@a1");
			result[2].Name.Should().Be.EqualTo("c1@c1");
			result[3].Name.Should().Be.EqualTo("d1@d1");
			result[4].Name.Should().Be.EqualTo("e1@e1");
			result[5].Name.Should().Be.EqualTo("f1@f1");
		}


		[Test]
		public void ShouldReturnSchedulesWithCorrectOrderForSpecificPageForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePersonWithGuid("a1","a1");
			PeopleSearchProvider.Add(person1);
			var person2 = PersonFactory.CreatePersonWithGuid("b1","b1");
			PeopleSearchProvider.Add(person2);
			var person3 = PersonFactory.CreatePersonWithGuid("c1","c1");
			PeopleSearchProvider.Add(person3);
			var person4 = PersonFactory.CreatePersonWithGuid("d1","d1");
			PeopleSearchProvider.Add(person4);
			var person5 = PersonFactory.CreatePersonWithGuid("e1","e1");
			PeopleSearchProvider.Add(person5);
			var person6 = PersonFactory.CreatePersonWithGuid("f1","f1");
			PeopleSearchProvider.Add(person6);

			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);

			var scheduleDay1 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person1,scenario);
			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
					scenario, ActivityFactory.CreateActivity("activity1",new Color()), new DateTimePeriod(2020,1,1,8,2020,1,1,9), ShiftCategoryFactory.CreateShiftCategory("test"));

			scheduleDay1.Add(pa1);

			var scheduleDay2 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person2,scenario);
			var pa2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
					scenario, ActivityFactory.CreateActivity("activity1",new Color()), new DateTimePeriod(2020,1,1,7,2020,1,1,9), ShiftCategoryFactory.CreateShiftCategory("test"));


			scheduleDay2.Add(pa2);

			var scheduleDay3 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person3,scenario);
			var pa3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
					scenario, ActivityFactory.CreateActivity("activity1",new Color()), new DateTimePeriod(2020,1,1,8,2020,1,1,9), ShiftCategoryFactory.CreateShiftCategory("test"));
			scheduleDay3.Add(pa3);

			var scheduleDayPrevious3 = ScheduleDayFactory.Create(new DateOnly(scheduleDate).AddDays(-1),person3,scenario);
			var paPrev = PersonAssignmentFactory.CreateAssignmentWithMainShift(person3,
				scenario, new DateTimePeriod(2019,12,31,20,2020,1,1,3));
			scheduleDayPrevious3.Add(paPrev);
			ScheduleProvider.AddScheduleDay(scheduleDayPrevious3);


			var scheduleDay4 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person4,scenario);
			var personAbsenceForPerson4 = PersonAbsenceFactory.CreatePersonAbsence(person4,scenario,
				new DateTimePeriod(2020,1,1,8,2020,1,1,17));


			var pa4 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person4,
					scenario, ActivityFactory.CreateActivity("activity1",new Color()), new DateTimePeriod(2020,1,1,8,2020,1,1,9), ShiftCategoryFactory.CreateShiftCategory("test"));

			scheduleDay4.Add(pa4);
			scheduleDay4.Add(personAbsenceForPerson4);

			var scheduleDay5 = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person5,scenario);
			scheduleDay5.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			ScheduleProvider.AddScheduleDay(scheduleDay4);
			ScheduleProvider.AddScheduleDay(scheduleDay5);
			ScheduleProvider.AddScheduleDay(scheduleDay3);
			ScheduleProvider.AddScheduleDay(scheduleDay1);
			ScheduleProvider.AddScheduleDay(scheduleDay2);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "firstName:a1 b1 c1 d1 e1 f1",
				Date = new DateOnly(scheduleDate),
				PageSize = 2,
				CurrentPageIndex = 2,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();

			result.Count.Should().Be.EqualTo(3);

			result[0].Name.Should().Be.EqualTo("c1@c1");
			result[1].Name.Should().Be.EqualTo("c1@c1");
			result[2].Name.Should().Be.EqualTo("d1@d1");
		}

		[Test]
		public void ShouldReturnCorrectProjectionForPeople()
		{
			var scheduleDate = new DateTime(2015,01,01,00,00,00,DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);

			var person1 = PersonFactory.CreatePersonWithGuid("person","1");
			var person2 = PersonFactory.CreatePersonWithGuid("person","2");
			PersonRepository.Add(person1);
			PersonRepository.Add(person2);

			var personAssignment1 = new PersonAssignment(person1,scenario,scheduleDateOnly);
			personAssignment1.AddActivity(new Activity("activity1"),new DateTimePeriod(scheduleDate.AddHours(7),scheduleDate.AddHours(18)));
			var scheduleDay1 = ScheduleDayFactory.Create(scheduleDateOnly,person1,scenario);
			scheduleDay1.Add(personAssignment1);


			var personAssignment2 = new PersonAssignment(person2,scenario,scheduleDateOnly);
			personAssignment2.AddActivity(new Activity("activity2"),new DateTimePeriod(scheduleDate.AddHours(8),scheduleDate.AddHours(18)));
			var scheduleDay2 = ScheduleDayFactory.Create(scheduleDateOnly,person2,scenario);
			scheduleDay2.Add(personAssignment2);

			ScheduleProvider.AddScheduleDay(scheduleDay1);
			ScheduleProvider.AddScheduleDay(scheduleDay2);

			var result = Target.GetSchedulesForPeople(new GetSchedulesForPeopleFormData
			{
				Date = scheduleDate,
				PersonIds = new List<Guid> { person1.Id.Value,person2.Id.Value }
			});

			result.Schedules.Count().Should().Be(2);
			result.Schedules.First().PersonId.Should().Be(person1.Id.Value.ToString());
			result.Schedules.Second().PersonId.Should().Be(person2.Id.Value.ToString());
			result.Schedules.First().Projection.First().Description.Should().Be("activity1");
			result.Schedules.Second().Projection.First().Description.Should().Be("activity2");
		}


		[Test]
		public void ShouldReturnInternalNotesWhenThereIsForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);
			scheduleDay.CreateAndAddNote("dummy notes");

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.InternalNotes.Should().Be("dummy notes");
		}

		[Test]
		public void ShouldReturnTimezoneWhenThereIsForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020,1,1,0,0,0,0,DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithGuid("Sherlock","Holmes");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test",true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate),person,scenario);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules(new SearchDaySchedulesFormData
			{
				Keyword = "Sherlock",
				Date = new DateOnly(scheduleDate),
				PageSize = 20,
				CurrentPageIndex = 1,
				IsOnlyAbsences = false
			}).Content.Schedules.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			new[] {
				"(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi",
				"(UTC+08:00) Beijing, Chongqing, Hong Kong SAR, Urumqi" // This is changed on new OS version, both values are correct.
			}.Should().Contain(schedule.Timezone.DisplayName);
			schedule.Timezone.IanaId.Should().Be("Asia/Shanghai");
		}

		public void Setup(ISystem system,IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeScheduleProvider>().For<IScheduleProvider>();
			system.UseTestDouble<FakeToggleManager>().For<IToggleManager>();
		}
	}
}
