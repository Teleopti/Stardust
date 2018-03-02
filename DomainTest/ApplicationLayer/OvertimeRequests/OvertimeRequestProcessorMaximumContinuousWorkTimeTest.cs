using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldApproveWhenOvertimeRequestMaximumContinuousWorkTimeIsDisabled()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = false
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldDenyWhenOvertimeRequestMaximumContinuousWorkTimeIsNull()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 6:00:00 PM - 7/17/2017 9:00:00 PM", TimeSpan.FromHours(3),
				TimeSpan.Zero);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldPendingWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithShiftBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 16);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(8),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Pending
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(16, 3);
			personRequest.ForcePending();
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 9:00:00 AM - 7/17/2017 7:00:00 PM", TimeSpan.FromHours(10),
				TimeSpan.FromHours(8));

			personRequest.IsPending.Should().Be.True();
			personRequest.GetMessage(new NoFormatting()).Equals(expectedDenyReason);
			personRequest.BrokenBusinessRules.Equals(BusinessRuleFlags.MaximumContinuousWorkTimeRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithShiftBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 16);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(8),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(16, 3);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 9:00:00 AM - 7/17/2017 7:00:00 PM", TimeSpan.FromHours(10),
				TimeSpan.FromHours(8));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldApproveWhenContinuousWorkTimeEqualsMaximumContinuousWorkTimeWithShiftBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 16);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(16, 3);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldApproveWhenContinuousWorkTimeLessThenMaximumContinuousWorkTimeOnFullDayAbsence()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 0, 2017, 7, 18, 0);
			var fullDayAbsence = new PersonAbsence(person, Scenario.Current(), new AbsenceLayer(new Absence(), period));
			ScheduleStorage.Add(fullDayAbsence);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(16, 3);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(17, 2);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldApproveWhenContinuousWorkTimeLessThenMaximumContinuousWorkTimeOnDayOff()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var pa = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, Scenario.Current(), new DateOnly(2017, 7, 17),
				new DayOffTemplate(new Description("for", "test")));
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(16, 3);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(16, 3);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldDenyWhenOvertimeExceedsMaximumContinuousWorkTimeOnEmptyDay()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 9, 2017, 7, 17, 16);
			var pa = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period);
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(2),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(16, 3);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 4:00:00 PM - 7/17/2017 7:00:00 PM", TimeSpan.FromHours(3),
				TimeSpan.FromHours(2));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithOvernightShiftBefore()
		{
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 16, 22, 2017, 7, 17, 1);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(4),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(1, 2);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/16/2017 10:00:00 PM - 7/17/2017 3:00:00 AM", TimeSpan.FromHours(5),
				TimeSpan.FromHours(4));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithShiftAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 10, 2017, 7, 17, 17);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(8),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(8, 2);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 5:00:00 PM", TimeSpan.FromHours(9),
				TimeSpan.FromHours(8));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithOvernightShiftAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 18, 2017, 7, 18, 1);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(8),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(16, 2);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 4:00:00 PM - 7/18/2017 1:00:00 AM", TimeSpan.FromHours(9),
				TimeSpan.FromHours(8));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldDenyWhenContinuousWorkTimeExceedsMaximumContinuousWorkTimeWithOvernightOvertime()
		{
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 18, 1, 2017, 7, 18, 8);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(8),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(23, 2);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 11:00:00 PM - 7/18/2017 8:00:00 AM", TimeSpan.FromHours(9),
				TimeSpan.FromHours(8));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(8),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(2, 1);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 1:00:00 AM - 7/17/2017 10:00:00 AM", TimeSpan.FromHours(9),
				TimeSpan.FromHours(8));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(9),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromMinutes(30)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequestInMinutes(17, 60);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 6:00:00 PM", TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(2)),
				TimeSpan.FromHours(9));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(24),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(0, 10);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/16/2017 11:00:00 PM - 7/18/2017 12:00:00 AM", TimeSpan.FromHours(25),
				TimeSpan.FromHours(24));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldApproveWhenRestTimeSatisfysMinimumRestTimeWithShiftBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 16);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(17, 3);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(20, 1);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldDenyWhenRestTimeIsLessThanMinimumRestTimeWithShiftBefore()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 16);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(2)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(17, 3);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 8:00:00 PM", TimeSpan.FromHours(11),
				TimeSpan.FromHours(10));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromMinutes(30)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequestInMinutes(17, 90);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 6:30:00 PM", TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(1)),
				TimeSpan.FromHours(10));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(2)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(10, 1);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/16/2017 11:00:00 PM - 7/17/2017 11:00:00 AM", TimeSpan.FromHours(11),
				TimeSpan.FromHours(10));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldApproveWhenRestTimeSatisfysMinimumRestTimeWithShiftAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 10, 2017, 7, 17, 20);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(1)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		public void ShouldDenyWhenRestTimeIsLessThanMinimumRestTimeWithShiftAfter()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var period = new DateTimePeriod(2017, 7, 17, 10, 2017, 7, 17, 20);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(2)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 8:00:00 PM", TimeSpan.FromHours(11),
				TimeSpan.FromHours(10));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromMinutes(30)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest, true);

			var expectedDenyReason = buildDenyReason("7/17/2017 8:00:00 AM - 7/17/2017 8:00:00 PM", TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(31)),
				TimeSpan.FromHours(10));

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
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

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
					OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(10),
					OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMinimumRestTimeThreshold = TimeSpan.FromHours(2)
				};
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(19, 1);
			getTarget().Process(personRequest, true);

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