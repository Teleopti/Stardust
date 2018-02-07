using System;
using NUnit.Framework;
using SharpTestsEx;
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

		private string buildDenyReason(string continuousWorkPeriod, TimeSpan continuousWorkTime, TimeSpan maximumContinuousWorkTime)
		{
			return string.Format(Resources.OvertimeRequestContinuousWorkTimeDenyReason,
				continuousWorkPeriod,
				DateHelper.HourMinutesString(continuousWorkTime.TotalMinutes),
				DateHelper.HourMinutesString(maximumContinuousWorkTime.TotalMinutes));
		}
	}
}