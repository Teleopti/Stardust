using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.WebTest.Areas.Search;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	[TeamScheduleTest]
	public class TeamScheduleControllerIntegrationTest
	{
		public TeamScheduleController Target;
		public FakeSchedulePersonProvider PersonProvider;
		public FakeScheduleProvider ScheduleProvider;
		public FakePermissionProvider PermissionProvider;
		public FakePeopleSearchProvider PeopleSearchProvider;

		[Test]
		public void TargetShouldNotBeNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsNoSchedule()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes"); // Refer to FakeCommonAgentNameProvider
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsMainShift()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();
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
		public void ShouldReturnCorrectProjectionWhenThereIsDayOff()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			scheduleDay.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();
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
		public void ShouldReturnCorrectProjectionWhenThereIsFullDayAbsenceOnly()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(scheduleDate.AddDays(-1)));
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));
			scheduleDay.Add(personAbsence);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2020, 1, 1));

			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();
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
		public void ShouldReturnCorrectProjectionWhenThereIsFullDayAbsenceAndShift()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 15));
			scheduleDay.Add(pa);
			scheduleDay.Add(personAbsence);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();
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
		public void ShouldReturnCorrectProjectionWhenThereIsFullDayAbsenceAndDayoff()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(scheduleDate.AddDays(-1)));
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));
			var pa = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(scheduleDate), new DayOffTemplate(new Description("testDayoff")));
			scheduleDay.Add(pa);
			scheduleDay.Add(personAbsence);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();
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
		public void ShouldReturnCorrectProjectionWhenThereIsFullDayAbsenceOnAContractDayOffDay()
		{
			var scheduleDate = new DateTime(2020, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(scheduleDate.AddDays(-1)));
			personPeriod.PersonContract.ContractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());
			person.AddPersonPeriod(personPeriod);
			var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(scheduleDate.AddDays(-1)));
			person.AddSchedulePeriod(schedulePeriod);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(2020, 1, 4, 8, 2020, 1, 4, 17));
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2020, 1, 4));
			scheduleDay.Add(pa);
			scheduleDay.Add(personAbsence);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();
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
		public void ShouldReturnCorrectProjectionWhenThereIsConfidentialAbsenceAndShiftAndNoPermissionForConfidential()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("pNoConfidential", "pNoConfidential");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var secretAbs = AbsenceFactory.CreateAbsence("secret");
			secretAbs.Confidential = true;
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", Color.White),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 15));
			scheduleDay.Add(pa);
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(secretAbs, new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 10)));

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();
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
		public void ShouldReturnCorrectProjectionWhenThereIsConfidentialAbsenceAndShiftAndWithPermissionForConfidential()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);
			PersonProvider.AddPersonWitViewConfidentialPermission(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var secretAbs = AbsenceFactory.CreateAbsence("secret");
			secretAbs.Confidential = true;
			secretAbs.DisplayColor = Color.Red;
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", Color.White),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 15));
			scheduleDay.Add(pa);
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(secretAbs, new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 10)));

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();
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
		public void ShouldReturnEmptyScheduleWhenScheduleIsUnpublishedAndNoViewUnpublishedSchedulePermission()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			person.WorkflowControlSet = new WorkflowControlSet("testWCS")
			{
				SchedulePublishedToDate = new DateTime(2019, 12, 30)
			};
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);
			PermissionProvider.PermitApplicationFunction(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, false);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();
			result.Count.Should().Be.EqualTo(1);

			var schedule = result.Single();
			schedule.Name.Should().Be.EqualTo("Sherlock@Holmes");
			schedule.IsFullDayAbsence.Should().Be.EqualTo(false);

			var projectionVm = schedule.Projection.ToList();
			projectionVm.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnScheduleForPreviousDayWhenThereIsOvernightShift()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			scheduleDay.Add(pa);
			ScheduleProvider.AddScheduleDay(scheduleDay);

			var scheduleDayPrevious = ScheduleDayFactory.Create(new DateOnly(scheduleDate).AddDays(-1), person, scenario);
			var paPrev = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				new DateTimePeriod(2019, 12, 31, 20, 2020, 1, 1, 3));
			scheduleDayPrevious.Add(paPrev);
			ScheduleProvider.AddScheduleDay(scheduleDayPrevious);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();

			result.Count.Should().Be.EqualTo(2);
			result.First().Date.Should().Be.EqualTo("2020-01-01");
			result.Second().Date.Should().Be.EqualTo("2019-12-31");
		}

		[Test]
		public void ShouldIndicateOvertimeActivity()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddOvertimeActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11),
				new MultiplicatorDefinitionSet("temp", MultiplicatorType.Overtime));
			scheduleDay.Add(pa);
			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();

			result.Count.Should().Be.EqualTo(1);
			var schedule = result.First();
			schedule.Date.Should().Be.EqualTo("2020-01-01");
			schedule.Projection.First().IsOvertime.Should().Be.EqualTo(false);
			schedule.Projection.Last().IsOvertime.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnSchedulesWithCorrectOrder()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePerson("a1", "a1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person1);
			var person2 = PersonFactory.CreatePerson("b1", "b1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person2);
			var person3 = PersonFactory.CreatePerson("c1", "c1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person3);
			var person4 = PersonFactory.CreatePerson("d1", "d1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person4);
			var person5 = PersonFactory.CreatePerson("e1", "e1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person5);
			var person6 = PersonFactory.CreatePerson("f1", "f1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person6);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);

			var scheduleDay1 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person1, scenario);
			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person1, new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

			scheduleDay1.Add(pa1);
			
			var scheduleDay2 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person2, scenario);
			var pa2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person2, new DateTimePeriod(2020, 1, 1, 7, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

				
			scheduleDay2.Add(pa2);
			
			var scheduleDay3 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person3, scenario);
			var pa3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person3, new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);
			scheduleDay3.Add(pa3);
			

			var scheduleDay4 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person4, scenario);
			var personAbsenceForPerson4 = PersonAbsenceFactory.CreatePersonAbsence(person4, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));


			var pa4 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person4, new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

			scheduleDay4.Add(pa4);
			scheduleDay4.Add(personAbsenceForPerson4);
			
			var scheduleDay5 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person5, scenario);
			scheduleDay5.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			ScheduleProvider.AddScheduleDay(scheduleDay4);
			ScheduleProvider.AddScheduleDay(scheduleDay5);
			ScheduleProvider.AddScheduleDay(scheduleDay3);
			ScheduleProvider.AddScheduleDay(scheduleDay1);
			ScheduleProvider.AddScheduleDay(scheduleDay2);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();

			result.Count.Should().Be.EqualTo(6);
			result[0].Name.Should().Be.EqualTo("b1@b1");
			result[1].Name.Should().Be.EqualTo("a1@a1");
			result[2].Name.Should().Be.EqualTo("c1@c1");
			result[3].Name.Should().Be.EqualTo("d1@d1");
			result[4].Name.Should().Be.EqualTo("e1@e1");
			result[5].Name.Should().Be.EqualTo("f1@f1");
		}

		[Test]
		public void ShouldReturnSchedulesWithCorrectOrderOnContractDayOffDay()
		{
			var scheduleDate = new DateTime(2020, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePerson("a1", "a1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person1);
			var person2 = PersonFactory.CreatePerson("b1", "b1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person2);
			var person3 = PersonFactory.CreatePerson("c1", "c1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person3);
			var person4 = PersonFactory.CreatePerson("d1", "d1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person4);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(scheduleDate.AddDays(-1)));
			personPeriod.PersonContract.ContractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());
			person4.AddPersonPeriod(personPeriod);
			var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(scheduleDate.AddDays(-1)));
			person4.AddSchedulePeriod(schedulePeriod);

			var person5 = PersonFactory.CreatePerson("e1", "e1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person5);
			var person6 = PersonFactory.CreatePerson("f1", "f1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person6);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);

			var scheduleDay1 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person1, scenario);
			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person1, new DateTimePeriod(2020, 1, 4, 8, 2020, 1, 4, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

			scheduleDay1.Add(pa1);
			
			var scheduleDay2 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person2, scenario);
			var pa2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person2, new DateTimePeriod(2020, 1, 4, 7, 2020, 1, 4, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

				
			scheduleDay2.Add(pa2);
			
			var scheduleDay3 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person3, scenario);
			var pa3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person3, new DateTimePeriod(2020, 1, 4, 8, 2020, 1, 4, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);
			scheduleDay3.Add(pa3);
			

			var scheduleDay4 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person4, scenario);
			var personAbsenceForPerson4 = PersonAbsenceFactory.CreatePersonAbsence(person4, scenario,
				new DateTimePeriod(2020, 1, 4, 8, 2020, 1, 4, 17));
			var pa4 = PersonAssignmentFactory.CreatePersonAssignment(person4, scenario, new DateOnly(2020, 1, 4));
			scheduleDay4.Add(pa4);

			scheduleDay4.Add(personAbsenceForPerson4);
			
			var scheduleDay5 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person5, scenario);
			scheduleDay5.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			ScheduleProvider.AddScheduleDay(scheduleDay4);
			ScheduleProvider.AddScheduleDay(scheduleDay5);
			ScheduleProvider.AddScheduleDay(scheduleDay3);
			ScheduleProvider.AddScheduleDay(scheduleDay1);
			ScheduleProvider.AddScheduleDay(scheduleDay2);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 20, 1).Content.GroupSchedule.ToList();

			result.Count.Should().Be.EqualTo(6);
			result[0].Name.Should().Be.EqualTo("b1@b1");
			result[1].Name.Should().Be.EqualTo("a1@a1");
			result[2].Name.Should().Be.EqualTo("c1@c1");
			result[3].Name.Should().Be.EqualTo("d1@d1");
			result[4].Name.Should().Be.EqualTo("e1@e1");
			result[5].Name.Should().Be.EqualTo("f1@f1");
		}


		[Test]
		public void ShouldReturnSchedulesWithCorrectOrderForSpecificPage()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePerson("a1", "a1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person1);
			var person2 = PersonFactory.CreatePerson("b1", "b1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person2);
			var person3 = PersonFactory.CreatePerson("c1", "c1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person3);
			var person4 = PersonFactory.CreatePerson("d1", "d1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person4);
			var person5 = PersonFactory.CreatePerson("e1", "e1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person5);
			var person6 = PersonFactory.CreatePerson("f1", "f1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person6);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);

			var scheduleDay1 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person1, scenario);
			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person1, new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

			scheduleDay1.Add(pa1);

			var scheduleDay2 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person2, scenario);
			var pa2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person2, new DateTimePeriod(2020, 1, 1, 7, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);


			scheduleDay2.Add(pa2);

			var scheduleDay3 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person3, scenario);
			var pa3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person3, new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);
			scheduleDay3.Add(pa3);

			var scheduleDayPrevious3 = ScheduleDayFactory.Create(new DateOnly(scheduleDate).AddDays(-1), person3, scenario);
			var paPrev = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person3,
				new DateTimePeriod(2019, 12, 31, 20, 2020, 1, 1, 3));
			scheduleDayPrevious3.Add(paPrev);
			ScheduleProvider.AddScheduleDay(scheduleDayPrevious3);


			var scheduleDay4 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person4, scenario);
			var personAbsenceForPerson4 = PersonAbsenceFactory.CreatePersonAbsence(person4, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));


			var pa4 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person4, new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

			scheduleDay4.Add(pa4);
			scheduleDay4.Add(personAbsenceForPerson4);

			var scheduleDay5 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person5, scenario);
			scheduleDay5.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			ScheduleProvider.AddScheduleDay(scheduleDay4);
			ScheduleProvider.AddScheduleDay(scheduleDay5);
			ScheduleProvider.AddScheduleDay(scheduleDay3);
			ScheduleProvider.AddScheduleDay(scheduleDay1);
			ScheduleProvider.AddScheduleDay(scheduleDay2);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate, 2, 2).Content.GroupSchedule.ToList();

			result.Count.Should().Be.EqualTo(3);

			result[0].Name.Should().Be.EqualTo("c1@c1");
			result[1].Name.Should().Be.EqualTo("c1@c1");
			result[2].Name.Should().Be.EqualTo("d1@d1");
		}
		
		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsNoScheduleForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PeopleSearchProvider.Add(person);

			var result = Target.SearchSchedules("Sherlock", scheduleDate, 20, 1).Content.Schedules.ToList();
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
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules("Sherlock", scheduleDate, 20, 1).Content.Schedules.ToList();
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
		public void ShouldReturnCorrectProjectionWhenThereIsDayOffForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			scheduleDay.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules("Sherlock", scheduleDate, 20, 1).Content.Schedules.ToList();
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
		public void ShouldReturnCorrectProjectionWhenThereIsFullDayAbsenceOnlyForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(scheduleDate.AddDays(-1)));
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));
			scheduleDay.Add(personAbsence);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2020, 1, 1));

			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules("Sherlock", scheduleDate, 20, 1).Content.Schedules.ToList();
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
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 15));
			scheduleDay.Add(pa);
			scheduleDay.Add(personAbsence);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules("Sherlock", scheduleDate, 20, 1).Content.Schedules.ToList();
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
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(scheduleDate.AddDays(-1)));
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));
			var pa = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(scheduleDate), new DayOffTemplate(new Description("testDayoff")));
			scheduleDay.Add(pa);
			scheduleDay.Add(personAbsence);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules("Sherlock", scheduleDate, 20, 1).Content.Schedules.ToList();
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
			var scheduleDate = new DateTime(2020, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(scheduleDate.AddDays(-1)));
			personPeriod.PersonContract.ContractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());
			person.AddPersonPeriod(personPeriod);
			var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(scheduleDate.AddDays(-1)));
			person.AddSchedulePeriod(schedulePeriod);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
				new DateTimePeriod(2020, 1, 4, 8, 2020, 1, 4, 17));
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(2020, 1, 4));
			scheduleDay.Add(pa);
			scheduleDay.Add(personAbsence);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules("Sherlock", scheduleDate, 20, 1).Content.Schedules.ToList();
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
		public void ShouldReturnCorrectProjectionWhenThereIsConfidentialAbsenceAndShiftAndNoPermissionForConfidentialForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("pNoConfidential", "pNoConfidential");
			PeopleSearchProvider.Add(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var secretAbs = AbsenceFactory.CreateAbsence("secret");
			secretAbs.Confidential = true;
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", Color.White),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 15));
			scheduleDay.Add(pa);
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(secretAbs, new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 10)));

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules("Sherlock", scheduleDate, 20, 1).Content.Schedules.ToList();
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
		public void ShouldReturnCorrectProjectionWhenThereIsConfidentialAbsenceAndShiftAndWithPermissionForConfidentialForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PeopleSearchProvider.Add(person);
			PeopleSearchProvider.AddPersonWithViewConfidentialPermission(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var secretAbs = AbsenceFactory.CreateAbsence("secret");
			secretAbs.Confidential = true;
			secretAbs.DisplayColor = Color.Red;
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", Color.White),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 15));
			scheduleDay.Add(pa);
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(secretAbs, new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 10)));

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules("Sherlock", scheduleDate, 20, 1).Content.Schedules.ToList();
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
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			person.WorkflowControlSet = new WorkflowControlSet("testWCS")
			{
				SchedulePublishedToDate = new DateTime(2019, 12, 30)
			};
			PeopleSearchProvider.Add(person);
			PermissionProvider.PermitApplicationFunction(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, false);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules("Sherlock", scheduleDate, 20, 1).Content.Schedules.ToList();
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
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			scheduleDay.Add(pa);
			ScheduleProvider.AddScheduleDay(scheduleDay);

			var scheduleDayPrevious = ScheduleDayFactory.Create(new DateOnly(scheduleDate).AddDays(-1), person, scenario);
			var paPrev = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person,
				new DateTimePeriod(2019, 12, 31, 20, 2020, 1, 1, 3));
			scheduleDayPrevious.Add(paPrev);
			ScheduleProvider.AddScheduleDay(scheduleDayPrevious);

			var result = Target.SearchSchedules("Sherlock", scheduleDate, 20, 1).Content.Schedules.ToList();

			result.Count.Should().Be.EqualTo(2);
			result.First().Date.Should().Be.EqualTo("2020-01-01");
			result.Second().Date.Should().Be.EqualTo("2019-12-31");
		}

		[Test]
		public void ShouldIndicateOvertimeActivityForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("Sherlock", "Holmes");
			PeopleSearchProvider.Add(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddOvertimeActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11),
				new MultiplicatorDefinitionSet("temp", MultiplicatorType.Overtime));
			scheduleDay.Add(pa);
			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.SearchSchedules("Sherlock", scheduleDate, 20, 1).Content.Schedules.ToList();

			result.Count.Should().Be.EqualTo(1);
			var schedule = result.First();
			schedule.Date.Should().Be.EqualTo("2020-01-01");
			schedule.Projection.First().IsOvertime.Should().Be.EqualTo(false);
			schedule.Projection.Last().IsOvertime.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnSchedulesWithCorrectOrderForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePerson("a1", "a1");
			PeopleSearchProvider.Add(person1);
			var person2 = PersonFactory.CreatePerson("b1", "b1");
			PeopleSearchProvider.Add(person2);
			var person3 = PersonFactory.CreatePerson("c1", "c1");
			PeopleSearchProvider.Add(person3);
			var person4 = PersonFactory.CreatePerson("d1", "d1");
			PeopleSearchProvider.Add(person4);
			var person5 = PersonFactory.CreatePerson("e1", "e1");
			PeopleSearchProvider.Add(person5);
			var person6 = PersonFactory.CreatePerson("f1", "f1");
			PeopleSearchProvider.Add(person6);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);

			var scheduleDay1 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person1, scenario);
			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person1, new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

			scheduleDay1.Add(pa1);
			
			var scheduleDay2 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person2, scenario);
			var pa2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person2, new DateTimePeriod(2020, 1, 1, 7, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

				
			scheduleDay2.Add(pa2);
			
			var scheduleDay3 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person3, scenario);
			var pa3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person3, new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);
			scheduleDay3.Add(pa3);
			

			var scheduleDay4 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person4, scenario);
			var personAbsenceForPerson4 = PersonAbsenceFactory.CreatePersonAbsence(person4, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));


			var pa4 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person4, new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

			scheduleDay4.Add(pa4);
			scheduleDay4.Add(personAbsenceForPerson4);
			
			var scheduleDay5 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person5, scenario);
			scheduleDay5.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			ScheduleProvider.AddScheduleDay(scheduleDay4);
			ScheduleProvider.AddScheduleDay(scheduleDay5);
			ScheduleProvider.AddScheduleDay(scheduleDay3);
			ScheduleProvider.AddScheduleDay(scheduleDay1);
			ScheduleProvider.AddScheduleDay(scheduleDay2);

			var result = Target.SearchSchedules("firstName:a1 b1 c1 d1 e1 f1", scheduleDate, 20, 1).Content.Schedules.ToList();

			result.Count.Should().Be.EqualTo(6);
			result[0].Name.Should().Be.EqualTo("b1@b1");
			result[1].Name.Should().Be.EqualTo("a1@a1");
			result[2].Name.Should().Be.EqualTo("c1@c1");
			result[3].Name.Should().Be.EqualTo("d1@d1");
			result[4].Name.Should().Be.EqualTo("e1@e1");
			result[5].Name.Should().Be.EqualTo("f1@f1");
		}

		[Test]
		public void ShouldReturnSchedulesWithCorrectOrderOnContractDayOffDayForScheduleSearch()
		{
			var scheduleDate = new DateTime(2020, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePerson("a1", "a1");
			PeopleSearchProvider.Add(person1);
			var person2 = PersonFactory.CreatePerson("b1", "b1");
			PeopleSearchProvider.Add(person2);
			var person3 = PersonFactory.CreatePerson("c1", "c1");
			PeopleSearchProvider.Add(person3);
			var person4 = PersonFactory.CreatePerson("d1", "d1");
			PeopleSearchProvider.Add(person4);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(scheduleDate.AddDays(-1)));
			personPeriod.PersonContract.ContractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());
			person4.AddPersonPeriod(personPeriod);
			var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(scheduleDate.AddDays(-1)));
			person4.AddSchedulePeriod(schedulePeriod);

			var person5 = PersonFactory.CreatePerson("e1", "e1");
			PeopleSearchProvider.Add(person5);
			var person6 = PersonFactory.CreatePerson("f1", "f1");
			PeopleSearchProvider.Add(person6);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);

			var scheduleDay1 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person1, scenario);
			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person1, new DateTimePeriod(2020, 1, 4, 8, 2020, 1, 4, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

			scheduleDay1.Add(pa1);
			
			var scheduleDay2 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person2, scenario);
			var pa2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person2, new DateTimePeriod(2020, 1, 4, 7, 2020, 1, 4, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

				
			scheduleDay2.Add(pa2);
			
			var scheduleDay3 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person3, scenario);
			var pa3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person3, new DateTimePeriod(2020, 1, 4, 8, 2020, 1, 4, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);
			scheduleDay3.Add(pa3);
			

			var scheduleDay4 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person4, scenario);
			var personAbsenceForPerson4 = PersonAbsenceFactory.CreatePersonAbsence(person4, scenario,
				new DateTimePeriod(2020, 1, 4, 8, 2020, 1, 4, 17));
			var pa4 = PersonAssignmentFactory.CreatePersonAssignment(person4, scenario, new DateOnly(2020, 1, 4));
			scheduleDay4.Add(pa4);

			scheduleDay4.Add(personAbsenceForPerson4);
			
			var scheduleDay5 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person5, scenario);
			scheduleDay5.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			ScheduleProvider.AddScheduleDay(scheduleDay4);
			ScheduleProvider.AddScheduleDay(scheduleDay5);
			ScheduleProvider.AddScheduleDay(scheduleDay3);
			ScheduleProvider.AddScheduleDay(scheduleDay1);
			ScheduleProvider.AddScheduleDay(scheduleDay2);

			var result = Target.SearchSchedules("firstName:a1 b1 c1 d1 e1 f1", scheduleDate, 20, 1).Content.Schedules.ToList();

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
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person1 = PersonFactory.CreatePerson("a1", "a1");
			PeopleSearchProvider.Add(person1);
			var person2 = PersonFactory.CreatePerson("b1", "b1");
			PeopleSearchProvider.Add(person2);
			var person3 = PersonFactory.CreatePerson("c1", "c1");
			PeopleSearchProvider.Add(person3);
			var person4 = PersonFactory.CreatePerson("d1", "d1");
			PeopleSearchProvider.Add(person4);
			var person5 = PersonFactory.CreatePerson("e1", "e1");
			PeopleSearchProvider.Add(person5);
			var person6 = PersonFactory.CreatePerson("f1", "f1");
			PeopleSearchProvider.Add(person6);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);

			var scheduleDay1 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person1, scenario);
			var pa1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person1, new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

			scheduleDay1.Add(pa1);

			var scheduleDay2 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person2, scenario);
			var pa2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person2, new DateTimePeriod(2020, 1, 1, 7, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);


			scheduleDay2.Add(pa2);

			var scheduleDay3 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person3, scenario);
			var pa3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person3, new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);
			scheduleDay3.Add(pa3);

			var scheduleDayPrevious3 = ScheduleDayFactory.Create(new DateOnly(scheduleDate).AddDays(-1), person3, scenario);
			var paPrev = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person3,
				new DateTimePeriod(2019, 12, 31, 20, 2020, 1, 1, 3));
			scheduleDayPrevious3.Add(paPrev);
			ScheduleProvider.AddScheduleDay(scheduleDayPrevious3);


			var scheduleDay4 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person4, scenario);
			var personAbsenceForPerson4 = PersonAbsenceFactory.CreatePersonAbsence(person4, scenario,
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));


			var pa4 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(ActivityFactory.CreateActivity("activity1", new Color()),
					person4, new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9), ShiftCategoryFactory.CreateShiftCategory("test"),
					scenario);

			scheduleDay4.Add(pa4);
			scheduleDay4.Add(personAbsenceForPerson4);

			var scheduleDay5 = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person5, scenario);
			scheduleDay5.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			ScheduleProvider.AddScheduleDay(scheduleDay4);
			ScheduleProvider.AddScheduleDay(scheduleDay5);
			ScheduleProvider.AddScheduleDay(scheduleDay3);
			ScheduleProvider.AddScheduleDay(scheduleDay1);
			ScheduleProvider.AddScheduleDay(scheduleDay2);

			var result = Target.SearchSchedules("firstName:a1 b1 c1 d1 e1 f1", scheduleDate, 2, 2).Content.Schedules.ToList();

			result.Count.Should().Be.EqualTo(3);

			result[0].Name.Should().Be.EqualTo("c1@c1");
			result[1].Name.Should().Be.EqualTo("c1@c1");
			result[2].Name.Should().Be.EqualTo("d1@d1");
		}
	}
		
	[TestFixture]
	internal class TeamScheduleControllerTest
	{
		[Test]
		public void ShouldGetFullDayAbsencePermission()
		{
			const bool expectedResult = true;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			principalAuthorization.Stub(x=>x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence)).Return(expectedResult);

			var target = new TeamScheduleController(null, null, null, principalAuthorization, null, null);
			var result = target.GetPermissions();

			result.Content.IsAddFullDayAbsenceAvailable.Should().Be.EqualTo(expectedResult);
		}		
		
		[Test]
		public void ShouldGetIntradayAbsencePermission()
		{
			const bool expectedResult = true;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence)).Return(expectedResult);

			var target = new TeamScheduleController(null, null, null, principalAuthorization, null, null);
			var result = target.GetPermissions();

			result.Content.IsAddIntradayAbsenceAvailable.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldAssignOperatePersonForAddFullDayAbsence()
		{
			var expectedPerson = new Person();
			expectedPerson.SetId(Guid.NewGuid());
			var loggonUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggonUser.Stub(x => x.CurrentUser()).Return(expectedPerson);

			var target = new TeamScheduleController(null, null, loggonUser, null, null, null);

			var form = new FullDayAbsenceForm {PersonIds = new List<Guid>(), TrackedCommandInfo = new TrackedCommandInfo()};
			target.AddFullDayAbsence(form);

			form.TrackedCommandInfo.OperatedPersonId.Should().Be.EqualTo(expectedPerson.Id.Value);
		}

		[Test]
		public void ShouldAddFullDayAbsenceForMoreThanOneAgent()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			var target = new TeamScheduleController(null, null, null, null, absencePersister, null);

			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var form = new FullDayAbsenceForm
			{
				PersonIds = new List<Guid>() { person1, person2 }
			};
			target.AddFullDayAbsence(form);

			absencePersister.AssertWasCalled(x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.PersonId == form.PersonIds.ToList()[0])));
			absencePersister.AssertWasCalled(x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.PersonId == form.PersonIds.ToList()[1])));
		}

		[Test]
		public void ShouldAddFullDayAbsenceThroughInputForm()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			var target = new TeamScheduleController(null, null, null, null, absencePersister, null);

			var form = new FullDayAbsenceForm
			{
				PersonIds = new List<Guid>{ Guid.NewGuid() },
				AbsenceId = Guid.NewGuid(),
				StartDate = DateTime.MinValue,
				EndDate = DateTime.MaxValue,
			};
			target.AddFullDayAbsence(form);

			absencePersister.AssertWasCalled(x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.AbsenceId == form.AbsenceId)));
			absencePersister.AssertWasCalled(x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.StartDate == form.StartDate)));
			absencePersister.AssertWasCalled(x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.EndDate == form.EndDate)));
		}

		[Test]
		public void ShouldAssignOperatePersonForAddIntradayAbsence()
		{
			var expectedPerson = new Person();
			expectedPerson.SetId(Guid.NewGuid());
			var loggonUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggonUser.Stub(x => x.CurrentUser()).Return(expectedPerson);

			var target = new TeamScheduleController(null, null, loggonUser, null, null, null);

			var form = new IntradayAbsenceForm { PersonIds = new List<Guid>(), TrackedCommandInfo = new TrackedCommandInfo() };
			target.AddIntradayAbsence(form);

			form.TrackedCommandInfo.OperatedPersonId.Should().Be.EqualTo(expectedPerson.Id.Value);
		}

		[Test]
		public void ShouldAddIntradayAbsenceForMoreThanOneAgent()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			var target = new TeamScheduleController(null, null, null, null, absencePersister, null);

			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var form = new IntradayAbsenceForm
			{
				StartTime = DateTime.MinValue,
				EndTime = DateTime.MaxValue,
				PersonIds = new List<Guid>() { person1, person2 }
			};
			target.AddIntradayAbsence(form);

			absencePersister.AssertWasCalled(x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.PersonId == form.PersonIds.ToList()[0])));
			absencePersister.AssertWasCalled(x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.PersonId == form.PersonIds.ToList()[1])));
		}

		[Test]
		public void ShouldAddIntradayAbsenceThroughInputForm()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			var target = new TeamScheduleController(null, null, null, null, absencePersister, null);

			var form = new IntradayAbsenceForm
			{
				PersonIds = new List<Guid> { Guid.NewGuid() },
				AbsenceId = Guid.NewGuid(),
				StartTime = DateTime.MinValue,
				EndTime = DateTime.MaxValue,
			};
			target.AddIntradayAbsence(form);

			absencePersister.AssertWasCalled(x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.AbsenceId == form.AbsenceId)));
			absencePersister.AssertWasCalled(x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.StartTime == form.StartTime)));
			absencePersister.AssertWasCalled(x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.EndTime == form.EndTime)));
		}

		[Test]
		public void ShouldReturnBadRequestWhenEndTimeEarlierThanStartTime()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			var target = new TeamScheduleController(null, null, null, null, absencePersister, null);

			var form = new IntradayAbsenceForm
			{
				StartTime = DateTime.MaxValue,
				EndTime = DateTime.MinValue,
			};
			var result = target.AddIntradayAbsence(form);

			result.Should().Be.OfType<BadRequestErrorMessageResult>();
			absencePersister.AssertWasNotCalled(x => x.PersistIntradayAbsence(null), y => y.IgnoreArguments());
		}

		[Test]
		public void ShouldUpdateAgentsPerPage()
		{
			const int expectedAgents = 30;
			var agentsPerPageSettingersisterAndProvider = MockRepository.GenerateMock<ISettingsPersisterAndProvider<AgentsPerPageSetting>>();
			var target = new TeamScheduleController(null, null, null, null, null, agentsPerPageSettingersisterAndProvider);

			target.UpdateAgentsPerPageSetting(expectedAgents);

			agentsPerPageSettingersisterAndProvider.AssertWasCalled(x=>x.Persist(Arg<AgentsPerPageSetting>.Matches(s => s.AgentsPerPage == expectedAgents)));
		}

		[Test]
		public void ShouldGetAgentsPerPage()
		{
			const int expectedAgents = 30;
			var loggonUser = new FakeLoggedOnUser();
			var agentsPerPageSettingersisterAndProvider = MockRepository.GenerateMock<ISettingsPersisterAndProvider<AgentsPerPageSetting>>();
			agentsPerPageSettingersisterAndProvider.Stub(x=>x.GetByOwner(loggonUser.CurrentUser())).Return(new AgentsPerPageSetting(){AgentsPerPage = expectedAgents});
			var target = new TeamScheduleController(null, null, loggonUser, null, null, agentsPerPageSettingersisterAndProvider);

			var result = target.GetAgentsPerPageSetting();

			result.Content.Agents.Should().Be.EqualTo(expectedAgents);
		}
	}
}
