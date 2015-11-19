using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	[TeamScheduleTest]
	public class TeamScheduleControllerTest
	{
		public TeamScheduleController Target;
		public FakeSchedulePersonProvider PersonProvider;
		public FakeScheduleProvider ScheduleProvider;
		public FakePermissionProvider PermissionProvider;

		[Test]
		public void TargetShouldNotBeNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsMainShift()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("p1", "p1");
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
			var projectionVm = result.Single().Projection.ToList();
			result.Count.Should().Be.EqualTo(1);
			result.Single().IsFullDayAbsence.Should().Be.EqualTo(false);
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
			var person = PersonFactory.CreatePerson("p1", "p1");
			PersonProvider.AddPersonWithMyTeamSchedulesPermission(person);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
			var pa = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, new DateOnly(scheduleDate));
			scheduleDay.CreateAndAddDayOff(DayOffFactory.CreateDayOff(new Description("test")));
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			scheduleDay.Add(pa);

			ScheduleProvider.AddScheduleDay(scheduleDay);

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
			var projectionVm = result.Single().Projection.ToList();
			result.Count.Should().Be.EqualTo(1);
			projectionVm.Count.Should().Be.EqualTo(0);

			result.Single().DayOff.DayOffName.Should().Be.EqualTo("test");
			result.Single().DayOff.Start.Should().Be.EqualTo("2020-01-01 00:00");
			result.Single().DayOff.Minutes.Should().Be.EqualTo(1440);
			result.Single().IsFullDayAbsence.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldReturnCorrectProjectionWhenThereIsFullDayAbsenceAndShift()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("p1", "p1");
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
			var projectionVm = result.Single().Projection.ToList();

			result.Count.Should().Be.EqualTo(1);
			result.Single().IsFullDayAbsence.Should().Be.EqualTo(true);

			projectionVm.Count.Should().Be.EqualTo(1);
			projectionVm.Single().Description.Should().Be.EqualTo(personAbsence.Layer.Payload.Description.Name);
			projectionVm.Single().Start.Should().Be.EqualTo("2020-01-01 09:00");
			projectionVm.Single().Minutes.Should().Be.EqualTo(360);
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
			var projectionVm = result.Single().Projection.ToList();
			result.Count.Should().Be.EqualTo(1);
			result.Single().IsFullDayAbsence.Should().Be.EqualTo(false);

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
			var person = PersonFactory.CreatePerson("p1", "p1");
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
			var projectionVm = result.Single().Projection.ToList();

			result.Count.Should().Be.EqualTo(1);
			result.Single().IsFullDayAbsence.Should().Be.EqualTo(false);

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
			var person = PersonFactory.CreatePerson("p1", "p1");
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
			var projectionVm = result.Single().Projection.ToList();
			result.Count.Should().Be.EqualTo(1);
			result.Single().IsFullDayAbsence.Should().Be.EqualTo(false);
			projectionVm.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnPreviousDaySScheduleWhenThereIsOvernightShift()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson("p1", "p1");
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();

			result.Count.Should().Be.EqualTo(2);
			result.First().Date.Should().Be.EqualTo("2020-01-01");
			result.Second().Date.Should().Be.EqualTo("2019-12-31");
		}

		[Test]
		public void ShouldReturnPreviousDaySScheduleWhenThereIsRegularActivityEndedOvernight()
		{
		}

		//[Test]
		//public void ShouldReturnCorrectProjectionWhenThereIsFullDayAbsenceOnly()
		//{
		//	var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		//	var person = PersonFactory.CreatePerson("p1", "p1");
		//	PersonProvider.Add(person);
		//	var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
		//	var scheduleDay = ScheduleDayFactory.Create(new DateOnly(scheduleDate), person, scenario);
		//	var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario,
		//		new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17));
		//	scheduleDay.Add(personAbsence);
		//	var pa = PersonAssignmentFactory.CreateEmptyAssignment(scenario, person,
		//		new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 17));

		//	scheduleDay.Add(pa);

		//	ScheduleProvider.AddScheduleDay(scheduleDay);

		//	var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
		//	var projectionVm = result.Single().Projection.ToList();
		//	result.Count.Should().Be.EqualTo(1);
		//	projectionVm.Count.Should().Be.EqualTo(1);
		//	projectionVm.Single().Description.Should().Be.EqualTo(personAbsence.Layer.Payload.Description.Name);
		//	projectionVm.Single().Start.Should().Be.EqualTo("2020-01-01 08:00");
		//	projectionVm.Single().Minutes.Should().Be.EqualTo(540);
		//	result.Single().IsFullDayAbsence.Should().Be.EqualTo(true);
		//}
	}
}
