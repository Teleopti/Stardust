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
		public void ShouldDenyWhenContinuousWorkTimeExceedsOvertimeRequestMaximumContinuousWorkTime()
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
		public void ShouldApproveWhenContinuousWorkTimeEqualsOvertimeRequestMaximumContinuousWorkTime()
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
		public void ShouldDenyWhenContinuousWorkTimeExceedsOvertimeRequestMaximumContinuousWorkTimeOnEmptyDay()
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
		public void ShouldDenyRequestWhenThereIsAnOverNightShift()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

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

		//[Test]
		//[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestMaxContinuousWorkTime_47964)]
		//public void ShouldDenyWhenContinuousWorkTimeExceedsOvertimeRequestMaximumContinuousWorkTimeWhenNoShiftBeforeOvertime()
		//{
		//	setupPerson(8, 21);
		//	setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

		//	var person = LoggedOnUser.CurrentUser();
		//	var period = new DateTimePeriod(2017, 7, 17, 17, 2017, 7, 17, 21);
		//	var pa = createMainPersonAssignment(person, period);
		//	ScheduleStorage.Add(pa);

		//	var workflowControlSet =
		//		new WorkflowControlSet
		//		{
		//			OvertimeRequestMaximumContinuousWorkTimeEnabled = true,
		//			OvertimeRequestMaximumContinuousWorkTime = TimeSpan.FromHours(4),
		//			OvertimeRequestMaximumContinuousWorkTimeHandleType = OvertimeValidationHandleType.Deny
		//		};
		//	person.WorkflowControlSet = workflowControlSet;

		//	var personRequest = createOvertimeRequest(16, 1);
		//	getTarget().Process(personRequest, true);

		//	var expectedDenyReason = buildDenyReason("7/17/2017 4:00:00 PM - 7/17/2017 9:00:00 PM", TimeSpan.FromHours(5),
		//		TimeSpan.FromHours(4));

		//	personRequest.IsDenied.Should().Be.True();
		//	personRequest.DenyReason.Should().Be.EqualTo(expectedDenyReason);
		//}

		private string buildDenyReason(string continuousWorkPeriod, TimeSpan continuousWorkTime, TimeSpan maximumContinuousWorkTime)
		{
			return string.Format(Resources.OvertimeRequestContinuousWorkTimeDenyReason,
				continuousWorkPeriod,
				DateHelper.HourMinutesString(continuousWorkTime.TotalMinutes),
				DateHelper.HourMinutesString(maximumContinuousWorkTime.TotalMinutes));
		}
	}
}