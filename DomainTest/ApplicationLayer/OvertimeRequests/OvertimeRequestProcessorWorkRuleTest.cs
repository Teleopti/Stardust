using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
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
			personRequest.DenyReason.Trim().Should().Be(string.Format(Resources.BusinessRuleNightlyRestRuleErrorMessage, "6:00", "7/13/2017", "7/14/2017", "5:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.NewNightlyRestRule);
		}

		[Test]
		public void ShouldDenyWhenViolateNightlyRestTimeRuleOnDayOff()
		{
			setupPerson(0, 24);
			var dateTime = new DateTime(2017, 7, 15, 0, 0, 0, DateTimeKind.Utc);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.Team.Site.AddOpenHour(new SiteOpenHour
			{
				Parent = personPeriod.Team.Site,
				TimePeriod = new TimePeriod(0, 0, 24, 0),
				WeekDay = dateTime.DayOfWeek,
				IsClosed = false
			});
			personPeriod.Team.Site.AddOpenHour(new SiteOpenHour
			{
				Parent = personPeriod.Team.Site,
				TimePeriod = new TimePeriod(0, 0, 24, 0),
				WeekDay = dateTime.AddDays(1).DayOfWeek,
				IsClosed = false
			});
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

			ScheduleStorage.Add(createMainPersonAssignmenDayoff(person, new DateOnly(dateTime)));
			ScheduleStorage.Add(createMainPersonAssignment(person, new DateTimePeriod(dateTime.AddDays(1).AddHours(8), dateTime.AddDays(1).AddHours(16))));

			var personRequest = createOvertimeRequest(dateTime, 48);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should().Be(string.Format(Resources.BusinessRuleNightlyRestRuleErrorMessage, "20:00", "7/15/2017", "7/16/2017", "-16:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.NewNightlyRestRule);
		}

		[Test]
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
				 string.Format(Resources.BusinessRuleNightlyRestRuleErrorMessage, "20:00", "7/13/2017", "7/14/2017", "11:00"), StringComparison.Ordinal) >
			 -1).Should().Be(true);
		}

		[Test]
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
			personRequest.DenyReason.Should()
				.Be(
					$"The week contains too much work time (27:00). Max is 10:00.\r\n{string.Format(Resources.BusinessRuleNightlyRestRuleErrorMessage, "6:00", "7/13/2017", "7/14/2017", "5:00")}");
			personRequest.BrokenBusinessRules.Should()
				.Be(BusinessRuleFlags.NewNightlyRestRule | BusinessRuleFlags.NewMaxWeekWorkTimeRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldApproveWhenNotVoilatingNightlyRestRuleAfterChangingBelongsToDateForOvernightShift()
		{
			setupPerson(0, 24);
			var person = LoggedOnUser.CurrentUser();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(60), TimeSpan.FromHours(6), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Pending,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 1d);

			var scheduleDataOne = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 12, 18, 2017, 7, 13, 2));
			var scheduleDataTwo = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 13, 18, 2017, 7, 14, 5));

			ScheduleStorage.Add(scheduleDataOne);
			ScheduleStorage.Add(scheduleDataTwo);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 2, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestChangeBelongsToDateForOverNightShift_74984)]
		public void ShouldDenyWhenViolatingNightlyRestRuleAfterChangingBelongsToDateForOvernightShift()
		{
			setupPerson(0, 24);
			var person = LoggedOnUser.CurrentUser();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(60), TimeSpan.FromHours(10), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 1d);

			var scheduleDataOne = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 12, 18, 2017, 7, 13, 2));
			var scheduleDataTwo = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 13, 18, 2017, 7, 14, 5));

			ScheduleStorage.Add(scheduleDataOne);
			ScheduleStorage.Add(scheduleDataTwo);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 2, 0, 0, DateTimeKind.Utc), 7);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should().Be(string.Format(Resources.BusinessRuleNightlyRestRuleErrorMessage, "10:00", "7/12/2017", "7/13/2017", "9:00"));
		}
	}
}