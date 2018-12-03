using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		public void ShouldApproveWhenOvertimeRequestMaximumContinuousWorkTimeIsDisabled()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = false;

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenOvertimeRequestMaximumContinuousWorkTimeIsNull()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);
			
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 6:00:00 PM - 7/17/2017 9:00:00 PM", TimeSpan.FromHours(3),
				TimeSpan.Zero);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldPendingWhenOvertimeRequestMaximumContinuousWorkTimeFailOptionIsSendToAdmin()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Pending;

			var personRequest = createOvertimeRequest(18, 3);
			getTarget(27).Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 6:00:00 PM - 7/17/2017 9:00:00 PM", TimeSpan.FromHours(3),
				TimeSpan.Zero);

			personRequest.IsPending.Should().Be.True();
			personRequest.GetMessage(new NoFormatting()).Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldPendingWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithShiftBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 16);
			var pa = createMainPersonAssignment(person, period);
			PersonAssignmentRepository.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(8);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType =
				OvertimeValidationHandleType.Pending;

			var personRequest = createOvertimeRequest(16, 3);
			personRequest.ForcePending();
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 9:00:00 AM - 7/17/2017 7:00:00 PM", TimeSpan.FromHours(10),
				TimeSpan.FromHours(8));

			personRequest.IsPending.Should().Be.True();
			personRequest.GetMessage(new NoFormatting()).Equals(expectedDenyReason);
			personRequest.BrokenBusinessRules.Equals(BusinessRuleFlags.MaximumContinuousWorkTimeRule);
		}

		[Test]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithShiftBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 16);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(8);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType =
				OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(16, 3);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 9:00:00 AM - 7/17/2017 7:00:00 PM", TimeSpan.FromHours(10),
				TimeSpan.FromHours(8));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldApproveWhenContinuousWorkTimeEqualsMaximumContinuousWorkTimeWithShiftBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 16);
			var pa = createMainPersonAssignment(person, period);
			PersonAssignmentRepository.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(16, 3);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenContinuousWorkTimeLessThenMaximumContinuousWorkTimeOnFullDayAbsence()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 0, 2017, 7, 18, 0);
			var fullDayAbsence = new PersonAbsence(person, Scenario.Current(), new AbsenceLayer(new Absence(), period));
			ScheduleStorage.Add(fullDayAbsence);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(16, 3);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenContinuousWorkTimeLessThenMaximumContinuousWorkTimeWithShiftContainingAbsenceBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 17);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			var absencePeriod = new DateTimePeriod(2017, 7, 17, 16, 2017, 7, 17, 17);
			var absence = new PersonAbsence(person, Scenario.Current(), new AbsenceLayer(new Absence(), absencePeriod));
			ScheduleStorage.Add(absence);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(17, 2);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenContinuousWorkTimeLessThenMaximumContinuousWorkTimeOnDayOff()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var pa = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, Scenario.Current(), new DateOnly(2017, 7, 17),
				new DayOffTemplate(new Description("for", "test")));
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(16, 3);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenContinuousWorkTimeLessThenMaximumContinuousWorkTimeWithOvertimeBeforeOnDayOff()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);
			var overtimePeriod = new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 16);
			var person = LoggedOnUser.CurrentUser();
			var pa = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, Scenario.Current(), new DateOnly(2017, 7, 17),
				new DayOffTemplate(new Description("for", "test")));
			pa.AddOvertimeActivity(new Activity(), overtimePeriod, new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime));
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(16, 3);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenOvertimeExceedsMaximumContinuousWorkTimeOnEmptyDay()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 16);
			var pa = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period);
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(2);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(16, 3);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 4:00:00 PM - 7/17/2017 7:00:00 PM", TimeSpan.FromHours(3),
				TimeSpan.FromHours(2));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithOvernightShiftBefore()
		{
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 16, 22, 2017, 7, 17, 1);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(4);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(1, 2);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/16/2017 10:00:00 PM - 7/17/2017 3:00:00 AM", TimeSpan.FromHours(5),
				TimeSpan.FromHours(4));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithShiftAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 10, 2017, 7, 17, 17);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(8);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(8, 2);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 5:00:00 PM", TimeSpan.FromHours(9),
				TimeSpan.FromHours(8));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithOvernightShiftAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 18, 2017, 7, 18, 1);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(8);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(16, 2);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 4:00:00 PM - 7/18/2017 1:00:00 AM", TimeSpan.FromHours(9),
				TimeSpan.FromHours(8));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithOvernightOvertime()
		{
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 18, 1, 2017, 7, 18, 8);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(8);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(23, 2);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 11:00:00 PM - 7/18/2017 8:00:00 AM", TimeSpan.FromHours(9),
				TimeSpan.FromHours(8));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithShiftBeforeAndAfter()
		{
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period1 = new DateTimePeriod(2017, 7, 17, 1, 2017, 7, 17, 2);
			var pa1 = createMainPersonAssignment(person, period1);

			var activity = ActivityFactory.CreateActivity("another activity");
			var period2 = new DateTimePeriod(2017, 7, 17, 3, 2017, 7, 17, 10);
			pa1.AddActivity(activity, period2);
			ScheduleStorage.Add(pa1);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(8);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(2, 1);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 1:00:00 AM - 7/17/2017 10:00:00 AM", TimeSpan.FromHours(9),
				TimeSpan.FromHours(8));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldDenyWhenRestTimeIsLessThanMinimumRestTimeWithMultipleActivitiesBeforeAndAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period1 = new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 16);
			var pa = createMainPersonAssignment(person, period1);

			var afterPeriod = new DateTimePeriod(new DateTime(2017, 7, 17, 16, 29, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 7, 17, 17, 0, 0, DateTimeKind.Utc));
			pa.AddActivity(new Activity(), afterPeriod);

			var beforePeriod = new DateTimePeriod(new DateTime(2017, 7, 17, 8, 00, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 7, 17, 8, 31, 0, DateTimeKind.Utc));
			pa.AddActivity(new Activity(), beforePeriod);

			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(9);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromMinutes(30);

			var personRequest = createOvertimeRequestInMinutes(17, 60);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 6:00:00 PM", TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(2)),
				TimeSpan.FromHours(9));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithOvernightShiftBeforeAndAfter()
		{
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period1 = new DateTimePeriod(2017, 7, 16, 23, 2017, 7, 17, 0);
			var pa1 = createMainPersonAssignment(person, period1);

			var period2 = new DateTimePeriod(2017, 7, 17, 10, 2017, 7, 18, 0);
			var pa2 = createMainPersonAssignment(person, period2);
			ScheduleStorage.Add(pa1);
			ScheduleStorage.Add(pa2);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(24);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;

			var personRequest = createOvertimeRequest(0, 10);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/16/2017 11:00:00 PM - 7/18/2017 12:00:00 AM", TimeSpan.FromHours(25),
				TimeSpan.FromHours(24));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldApproveWhenRestTimeSatisfysMinimumRestTimeWithShiftBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 16);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1);

			var personRequest = createOvertimeRequest(17, 3);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenRestTimeSatisfysMinimumRestTimeWithShiftAndLunchBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 0, 2017, 7, 17, 10);
			var pa = createMainPersonAssignment(person, period);
			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.ReportLevelDetail = ReportLevelDetail.Lunch;
			pa.AddActivity(lunch, new DateTimePeriod(2017, 7, 17, 10, 2017, 7, 17, 11));
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithShiftAndLunchBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(new DateTime(2017, 7, 17, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 7, 17, 10, 01, 0, DateTimeKind.Utc));
			var pa = createMainPersonAssignment(person, period);

			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.ReportLevelDetail = ReportLevelDetail.Lunch;
			pa.AddActivity(lunch, new DateTimePeriod(new DateTime(2017, 7, 17, 10, 1, 0, DateTimeKind.Utc),
				new DateTime(2017, 7, 17, 10, 30, 0, DateTimeKind.Utc)));

			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();

			var expectedDenyReason = buildDenyReason("7/17/2017 12:00:00 AM - 7/17/2017 12:00:00 PM", TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(1)),
				TimeSpan.FromHours(10));

			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldApproveWhenRestTimeSatisfysMinimumRestTimeWithShiftAndShortBreakBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 10);
			var pa = createMainPersonAssignment(person, period);
			var shortBreak = ActivityFactory.CreateActivity("short break");
			shortBreak.ReportLevelDetail = ReportLevelDetail.ShortBreak;
			pa.AddActivity(shortBreak, new DateTimePeriod(2017, 7, 17, 10, 2017, 7, 17, 20));
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1);

			var personRequest = createOvertimeRequest(20, 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}


		[Test]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithShiftAndShortBreakBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 12);
			var pa = createMainPersonAssignment(person, period);
			var shortBreak = ActivityFactory.CreateActivity("short break");
			shortBreak.ReportLevelDetail = ReportLevelDetail.ShortBreak;
			pa.AddActivity(shortBreak, new DateTimePeriod(new DateTime(2017, 7, 17, 12, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 7, 17, 12, 30, 0, DateTimeKind.Utc)));
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(4);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(1));

			var personRequest = createOvertimeRequest(13, 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 2:00:00 PM", TimeSpan.FromHours(5),
				TimeSpan.FromHours(4));

			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldDenyWhenRestTimeIsLessThanMinimumRestTimeWithShiftBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 16);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(2);

			var personRequest = createOvertimeRequest(17, 3);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 8:00:00 PM", TimeSpan.FromHours(11),
				TimeSpan.FromHours(10));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldDenyWhenRestTimeIsLessThanMinimumRestTimeWithMultipleActivitiesBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(new DateTime(2017, 7, 17, 7, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 7, 17, 7, 10, 0, DateTimeKind.Utc));
			var pa = createMainPersonAssignment(person, period);

			var period1 = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 16);
			pa.AddActivity(new Activity(), period1);

			var period2 = new DateTimePeriod(new DateTime(2017, 7, 17, 16, 29, 0, DateTimeKind.Utc),
				new DateTime(2017, 7, 17, 17, 0, 0, DateTimeKind.Utc));
			pa.AddActivity(new Activity(), period2);

			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromMinutes(30);

			var personRequest = createOvertimeRequestInMinutes(17, 90);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 6:30:00 PM", TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(1)),
				TimeSpan.FromHours(10));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldDenyWhenRestTimeIsLessThanMinimumRestTimeWithOverNightShiftBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 16, 23, 2017, 7, 17, 7);
			var pa = createMainPersonAssignment(person, period);

			var phone = ActivityFactory.CreateActivity("phone");
			pa.AddActivity(phone, new DateTimePeriod(2017, 7, 17, 7, 2017, 7, 17, 9));

			var shortBreak = ActivityFactory.CreateActivity("short break");
			shortBreak.ReportLevelDetail = ReportLevelDetail.ShortBreak;
			pa.AddActivity(shortBreak, new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 10));
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(2);

			var personRequest = createOvertimeRequest(10, 1);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/16/2017 11:00:00 PM - 7/17/2017 11:00:00 AM", TimeSpan.FromHours(11),
				TimeSpan.FromHours(10));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldApproveWhenRestTimeSatisfysMinimumRestTimeWithShiftAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 10, 2017, 7, 17, 20);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1);

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenRestTimeSatisfysMinimumRestTimeWithShiftAndLunchAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 10, 2017, 7, 17, 20);
			var pa = createMainPersonAssignment(person, period);
			var lunch = ActivityFactory.CreateActivity("lunch");
			lunch.ReportLevelDetail = ReportLevelDetail.Lunch;
			pa.AddActivity(lunch, new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 10));
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1);

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenRestTimeSatisfysMinimumRestTimeWithShiftAndShortBreakAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 10, 2017, 7, 17, 20);
			var pa = createMainPersonAssignment(person, period);
			var shortBreak = ActivityFactory.CreateActivity("short break");
			shortBreak.ReportLevelDetail = ReportLevelDetail.ShortBreak;
			pa.AddActivity(shortBreak, new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 10));
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1);

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenRestTimeIsLessThanMinimumRestTimeWithShiftAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 10, 2017, 7, 17, 20);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(2);

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 8:00:00 PM", TimeSpan.FromHours(11),
				TimeSpan.FromHours(10));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldDenyWhenRestTimeIsLessThanMinimumRestTimeWithMultipleActivitiesAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();

			var period = new DateTimePeriod(new DateTime(2017, 7, 17, 9, 0, 0, DateTimeKind.Utc),
				new DateTime(2017, 7, 17, 9, 31, 0, DateTimeKind.Utc));

			var pa = createMainPersonAssignment(person, period);

			var period1 = new DateTimePeriod(2017, 7, 17, 10, 2017, 7, 17, 20);
			pa.AddActivity(new Activity(), period1);

			var period2 = new DateTimePeriod(new DateTime(2017, 7, 17, 20, 31, 0, DateTimeKind.Utc),
				new DateTime(2017, 7, 17, 21, 0, 0, DateTimeKind.Utc));
			pa.AddActivity(new Activity(), period2);

			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromMinutes(30);

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 8:00:00 PM", TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(31)),
				TimeSpan.FromHours(10));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		public void ShouldDenyWhenRestTimeIsLessThanMinimumRestTimeWithOverNightShiftAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 23, 2017, 7, 18, 7);
			var pa = createMainPersonAssignment(person, period);

			var phone = ActivityFactory.CreateActivity("phone");
			pa.AddActivity(phone, new DateTimePeriod(2017, 7, 17, 21, 2017, 7, 17, 23));

			var shortBreak = ActivityFactory.CreateActivity("short break");
			shortBreak.ReportLevelDetail = ReportLevelDetail.ShortBreak;
			pa.AddActivity(shortBreak, new DateTimePeriod(2017, 7, 17, 20, 2017, 7, 17, 21));
			ScheduleStorage.Add(pa);

			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10);
			person.WorkflowControlSet.OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(2);

			var personRequest = createOvertimeRequest(19, 1);
			getTarget().Process(personRequest);

			var expectedDenyReason = buildDenyReason("7/17/2017 7:00:00 PM - 7/18/2017 7:00:00 AM", TimeSpan.FromHours(11),
				TimeSpan.FromHours(10));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		private string buildDenyReason(string continuousWorkPeriod, TimeSpan continuousWorkTime, TimeSpan maximumContinuousWorkTime)
		{
			return string.Format(Resources.OvertimeRequestContinuousWorkTimeDenyReason,
				continuousWorkPeriod,
				DateHelper.HourMinutesString(continuousWorkTime.TotalMinutes),
				DateHelper.HourMinutesString(maximumContinuousWorkTime.TotalMinutes));
		}
	}
}