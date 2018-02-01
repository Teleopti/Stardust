using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldDenyWhenViolateMaxWeekWorkTimeRule()
		{
			setupPerson(8, 21);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(8),
				TimeSpan.FromHours(9), TimeSpan.FromHours(10), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var pa = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 13, 8, 2017, 7, 13, 16));
			ScheduleStorage.Add(pa);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 2);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should().Be("The week contains too much work time (10:00). Max is 09:00.");
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.NewMaxWeekWorkTimeRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldNotDenyWhenViolateMaxWeekWorkTimeRuleWithToggle46417Off()
		{
			setupPerson(8, 21);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(8),
				TimeSpan.FromHours(9), TimeSpan.FromHours(10), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var pa = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 13, 8, 2017, 7, 13, 16));
			ScheduleStorage.Add(pa);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 2);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldNotDenyWhenViolateMaxWeekWorkTimeRuleAndHandleTypeIsPending()
		{
			setupPerson(8, 21);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(9), TimeSpan.FromHours(10), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Pending,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var pa = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 13, 8, 2017, 7, 13, 16));
			ScheduleStorage.Add(pa);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 2);
			getTarget().Process(personRequest);

			personRequest.IsPending.Should().Be.True();
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.NewMaxWeekWorkTimeRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldPendingWhenViolateMaxWeekWorkTimeRuleAndHandleTypeIsPendingAndAutoGrantIsYes()
		{
			setupPerson(8, 21);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(9), TimeSpan.FromHours(10), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Pending,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var pa = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 13, 8, 2017, 7, 13, 16));
			ScheduleStorage.Add(pa);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 2);
			getTarget().Process(personRequest);

			personRequest.IsPending.Should().Be.True();
			personRequest.GetMessage(new NoFormatting()).Trim().Should().Be("The week contains too much work time (10:00). Max is 09:00.");
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.NewMaxWeekWorkTimeRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldApproveWhenSatisfyMaxWeekWorkTimeRule()
		{
			setupPerson(8, 21);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(41), TimeSpan.FromHours(10), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			for (int i = 0; i < 5; i++)
			{
				var day = 10 + i;
				var pa = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, day, 8, 2017, 7, day, 16));
				ScheduleStorage.Add(pa);
			}

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldDenyWhenViolateNightlyRestTimeRule()
		{
			setupPerson(8, 21);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(60), TimeSpan.FromHours(6), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var scheduleDataOne = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 13, 8, 2017, 7, 13, 16));
			var scheduleDataTwo = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 14, 8, 2017, 7, 14, 16));
			ScheduleStorage.Add(scheduleDataOne);
			ScheduleStorage.Add(scheduleDataTwo);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 11);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should().Be("There must be a daily rest of at least 6:00 hours between 2 shifts. Between 7/13/2017 and 7/14/2017 there are only 5:00 hours.");
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.NewNightlyRestRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldDenyWhenViolateNightlyRestTimeRuleOnDayOff()
		{
			setupPerson(8, 21);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(60), TimeSpan.FromHours(20), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			ScheduleStorage.Add(createMainPersonAssignmenDayoff(person, new DateOnly(2017, 7, 15)));
			ScheduleStorage.Add(createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 16, 8, 2017, 7, 16, 16)));

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 15, 0, 0, 0, DateTimeKind.Utc), 48);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should().Be("There must be a daily rest of at least 20:00 hours between 2 shifts. Between 7/15/2017 and 7/16/2017 there are only -16:00 hours.");
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.NewNightlyRestRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldApproveWhenSatisfyNightlyRestTimeRule()
		{
			setupPerson(8, 21);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(60), TimeSpan.FromHours(6), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var scheduleDataOne = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 13, 8, 2017, 7, 13, 16));
			var scheduleDataTwo = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 14, 8, 2017, 7, 14, 16));
			ScheduleStorage.Add(scheduleDataOne);
			ScheduleStorage.Add(scheduleDataTwo);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 10);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldShowPeriodUsingAgentTimezoneWhenViolateNightlyRestTimeRule()
		{
			setupPerson(8, 21);
			var person = LoggedOnUser.CurrentUser();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(25), TimeSpan.FromHours(20), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var timezone = person.PermissionInformation.DefaultTimeZone();
			for (var i = 0; i < 5; i++)
			{
				var day = 10 + i;
				var startTime = TimeZoneHelper.ConvertToUtc(new DateTime(2017, 7, day, 14, 0, 0), timezone);
				var endTime = TimeZoneHelper.ConvertToUtc(new DateTime(2017, 7, day, 23, 0, 0), timezone);
				var pa = createMainPersonAssignment(person, new DateTimePeriod(startTime, endTime));
				ScheduleStorage.Add(pa);
			}

			var requestDate = TimeZoneHelper.ConvertToUtc(new DateTime(2017, 7, 13, 23, 0, 0), timezone);
			var personRequest = createOvertimeRequest(requestDate, 4);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();

			var denyReason = personRequest.DenyReason;
			(denyReason.IndexOf(
				 "There must be a daily rest of at least 20:00 hours between 2 shifts. Between 7/13/2017 and 7/14/2017 there are only 11:00 hours.", StringComparison.Ordinal) >
			 -1).Should().Be(true);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldDenyWhenViolateWeeklyRestTimeRule()
		{
			setupPerson(0, 24);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(168), TimeSpan.FromHours(6), TimeSpan.FromHours(30));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 17, 4, 0, 0, DateTimeKind.Utc), 168);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should().Be("The week does not have the stipulated (30:00) weekly rest.");
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MinWeeklyRestRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldShowFirstDenyReasonWhenViolateMaxtimePerWeekAndNightlyRestTimeRule()
		{
			setupPerson(8, 21);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(10), TimeSpan.FromHours(6), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var scheduleDataOne = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 13, 8, 2017, 7, 13, 16));
			var scheduleDataTwo = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 14, 8, 2017, 7, 14, 16));
			ScheduleStorage.Add(scheduleDataOne);
			ScheduleStorage.Add(scheduleDataTwo);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 11);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("The week contains too much work time (27:00). Max is 10:00.\r\nThere must be a daily rest of at least 6:00 hours between 2 shifts. Between 7/13/2017 and 7/14/2017 there are only 5:00 hours.");
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.NewNightlyRestRule | BusinessRuleFlags.NewMaxWeekWorkTimeRule);
		}
	}
}