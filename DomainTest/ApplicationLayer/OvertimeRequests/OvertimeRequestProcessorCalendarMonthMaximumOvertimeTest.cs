using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		public void ShouldApproveWhenOvertimeRequestMaximumTimeIsDisabled()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumTimeEnabled = false;

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenOvertimeRequestMaximumTimeIsZero()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			var workflowControlSet = person.WorkflowControlSet;
			workflowControlSet.OvertimeRequestMaximumTimeEnabled = true;
			workflowControlSet.OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny;
			workflowControlSet.OvertimeRequestMaximumTime = TimeSpan.Zero;

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "July", "03:00", "00:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		public void ShouldApproveWhenRequestTimeIsLessThanMaximumTime()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMaximumTime = TimeSpan.FromHours(10);


			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenRequestTimeEqualToMaximumTime()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMaximumTime = TimeSpan.FromHours(3);


			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenViolateOvertimeRequestMaximumTime()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMaximumTime = TimeSpan.FromHours(2);

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "July", "03:00", "02:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		public void ShouldNotDenyWhenOvertimeRequestMaximumTimeHandleTypeIsSendToAdministrator()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Pending;
			person.WorkflowControlSet.OvertimeRequestMaximumTime = TimeSpan.FromHours(2);


			var personRequest = createOvertimeRequest(18, 3);
			personRequest.Pending();
			getTarget().Process(personRequest);

			personRequest.IsPending.Should().Be.True();
			personRequest.GetMessage(new NoFormatting()).Trim().Should()
				.Be(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "July", "03:00", "02:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		public void ShouldDenyWhenTotalOvertimeOfMonthExceedsMaximumTime()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMaximumTime = TimeSpan.FromHours(10);

			var period = new DateTimePeriod(2017, 7, 2, 8, 2017, 7, 2, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period).WithId();
			var main = ActivityFactory.CreateActivity("phone").WithId();
			main.AllowOverwrite = true;
			main.InWorkTime = true;
			assignment.AddOvertimeActivity(main, period, new MultiplicatorDefinitionSet("ot", MultiplicatorType.Overtime));
			ScheduleStorage.Add(assignment);

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "July", "13:00", "10:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		public void ShouldApproveWhenRequestTimeIsCrossMonth()
		{
			Now.Is(new DateTime(2017, 7, 30, 22, 0, 0, DateTimeKind.Utc));
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMaximumTime = TimeSpan.FromHours(13);

			var period = new DateTimePeriod(2017, 7, 2, 8, 2017, 7, 2, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period).WithId();
			var main = ActivityFactory.CreateActivity("phone").WithId();
			main.AllowOverwrite = true;
			main.InWorkTime = true;
			assignment.AddOvertimeActivity(main, period, new MultiplicatorDefinitionSet("ot", MultiplicatorType.Overtime));
			ScheduleStorage.Add(assignment);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 31, 22, 0, 0, DateTimeKind.Utc), 4);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenOvertimeOfSecondMonthExceedsMaximumTime()
		{
			Now.Is(new DateTime(2017, 7, 30, 22, 0, 0, DateTimeKind.Utc));
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMaximumTime = TimeSpan.FromHours(13);

			var period = new DateTimePeriod(2017, 7, 2, 8, 2017, 7, 2, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period).WithId();
			var main = ActivityFactory.CreateActivity("phone").WithId();
			main.AllowOverwrite = true;
			main.InWorkTime = true;
			assignment.AddOvertimeActivity(main, period, new MultiplicatorDefinitionSet("ot", MultiplicatorType.Overtime));
			ScheduleStorage.Add(assignment);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 31, 22, 0, 0, DateTimeKind.Utc), 16);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "August", "14:00", "13:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		public void ShouldDenyWhenOvertimeOfBothMonthsExceedsMaximumTime()
		{
			Now.Is(new DateTime(2017, 7, 30, 22, 0, 0, DateTimeKind.Utc));
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMaximumTime = TimeSpan.FromHours(11);

			var period = new DateTimePeriod(2017, 7, 2, 8, 2017, 7, 2, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period).WithId();
			var main = ActivityFactory.CreateActivity("phone").WithId();
			main.AllowOverwrite = true;
			main.InWorkTime = true;
			assignment.AddOvertimeActivity(main, period, new MultiplicatorDefinitionSet("ot", MultiplicatorType.Overtime));
			ScheduleStorage.Add(assignment);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 31, 22, 0, 0, DateTimeKind.Utc), 16);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should()
				.Contain(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "July", "12:00", "11:00"));
			personRequest.DenyReason.Trim().Should()
				.Contain(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "August", "14:00", "11:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		public void ShouldAddCrossMonthOvertimeStartsFromLastMonth()
		{
			Now.Is(new DateTime(2017, 12, 25, 08, 0, 0, DateTimeKind.Utc));
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMaximumTime = TimeSpan.FromHours(1);
			var period = new DateTimePeriod(2017, 12, 31, 8, 2017, 12, 31, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period).WithId();
			ScheduleStorage.Add(assignment);

			var corssMonthPersonRequest = createOvertimeRequest(new DateTime(2017, 12, 31, 23, 0, 0, DateTimeKind.Utc), 2);
			getTarget().Process(corssMonthPersonRequest);

			corssMonthPersonRequest.IsApproved.Should().Be.True();

			var nextMonthPersonRequest = createOvertimeRequest(new DateTime(2018, 1, 1, 1, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(nextMonthPersonRequest);

			nextMonthPersonRequest.IsDenied.Should().Be.True();
			nextMonthPersonRequest.DenyReason.Should().Be(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "January", "02:00", "01:00"));
			nextMonthPersonRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		public void ShouldSubtractCrossMonthOvertimeEndsInNextMonth()
		{
			Now.Is(new DateTime(2017, 12, 25, 08, 0, 0, DateTimeKind.Utc));
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet.OvertimeRequestMaximumTimeEnabled = true;
			person.WorkflowControlSet.OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny;
			person.WorkflowControlSet.OvertimeRequestMaximumTime = TimeSpan.FromHours(2);

			var period = new DateTimePeriod(2017, 12, 31, 8, 2017, 12, 31, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period).WithId();
			ScheduleStorage.Add(assignment);

			var corssMonthPersonRequest = createOvertimeRequest(new DateTime(2017, 12, 31, 23, 0, 0, DateTimeKind.Utc), 2);
			getTarget().Process(corssMonthPersonRequest);

			corssMonthPersonRequest.IsApproved.Should().Be.True();

			var nextMonthPersonRequest = createOvertimeRequest(new DateTime(2017, 12, 31, 22, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(nextMonthPersonRequest);

			nextMonthPersonRequest.IsApproved.Should().Be.True();
		}
	}
}