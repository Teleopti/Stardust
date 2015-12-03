using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();
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

			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();

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
			
			var result = Target.GroupScheduleNoReadModel(Guid.NewGuid(), scheduleDate).Content.ToList();

			result.Count.Should().Be.EqualTo(1);
			var schedule = result.First();
			schedule.Date.Should().Be.EqualTo("2020-01-01");
			schedule.Projection.First().IsOvertime.Should().Be.EqualTo(false);
			schedule.Projection.Last().IsOvertime.Should().Be.EqualTo(true);
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

			var target = new TeamScheduleController(null, null, null, principalAuthorization, null);
			var result = target.GetPermissions();

			result.Content.IsAddFullDayAbsenceAvailable.Should().Be.EqualTo(expectedResult);
		}		
		
		[Test]
		public void ShouldGetIntradayAbsencePermission()
		{
			const bool expectedResult = true;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence)).Return(expectedResult);

			var target = new TeamScheduleController(null, null, null, principalAuthorization, null);
			var result = target.GetPermissions();

			result.Content.IsAddIntradayAbsenceAvailable.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldAssignOperatePersionForAddFullDayAbsence()
		{
			var expectedPerson = new Person();
			expectedPerson.SetId(Guid.NewGuid());
			var loggonUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggonUser.Stub(x => x.CurrentUser()).Return(expectedPerson);
			
			var target = new TeamScheduleController(null, null, loggonUser, null, null);

			var form = new FullDayAbsenceForm {PersonIds = new List<Guid>(), TrackedCommandInfo = new TrackedCommandInfo()};
			target.AddFullDayAbsence(form);

			form.TrackedCommandInfo.OperatedPersonId.Should().Be.EqualTo(expectedPerson.Id.Value);
		}

		[Test]
		public void ShouldAddFullDayAbsenceForMoreThanOneAgent()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new TeamScheduleController(null, null, null, null, commandDispatcher);

			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var form = new FullDayAbsenceForm
			{
				PersonIds = new List<Guid>() { person1, person2 }
			};
			target.AddFullDayAbsence(form);

			commandDispatcher.AssertWasCalled(x => x.Execute(Arg<AddFullDayAbsenceCommand>.Matches(s => s.PersonId == form.PersonIds.ToList()[0])));
			commandDispatcher.AssertWasCalled(x => x.Execute(Arg<AddFullDayAbsenceCommand>.Matches(s => s.PersonId == form.PersonIds.ToList()[1])));
		}

		[Test]
		public void ShouldAddFullDayAbsenceThroughInputForm()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new TeamScheduleController(null, null, null, null, commandDispatcher);

			var form = new FullDayAbsenceForm
			{
				PersonIds = new List<Guid>{ Guid.NewGuid() },
				AbsenceId = Guid.NewGuid(),
				StartDate = DateTime.MinValue,
				EndDate = DateTime.MaxValue,
			};
			target.AddFullDayAbsence(form);

			commandDispatcher.AssertWasCalled(x => x.Execute(Arg<AddFullDayAbsenceCommand>.Matches(s => s.AbsenceId == form.AbsenceId)));
			commandDispatcher.AssertWasCalled(x => x.Execute(Arg<AddFullDayAbsenceCommand>.Matches(s => s.StartDate == form.StartDate)));
			commandDispatcher.AssertWasCalled(x => x.Execute(Arg<AddFullDayAbsenceCommand>.Matches(s => s.EndDate == form.EndDate)));
		}
	}
}
