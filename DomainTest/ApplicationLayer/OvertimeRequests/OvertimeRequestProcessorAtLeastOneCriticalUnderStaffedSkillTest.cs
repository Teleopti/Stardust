using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
		public void ShouldApproveWhenAtLeastOneSkillIsCriticalUnderStaffed()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 24)
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var notUnderStaffingSkill = createSkill("notUnderStaffingSkill", null, timeZone);
			var criticalUnderStaffingSkill = createSkill("criticalUnderStaffingSkill", null, timeZone);

			var personSkill1 = createPersonSkill(activity1, notUnderStaffingSkill);
			var personSkill2 = createPersonSkill(activity2, criticalUnderStaffingSkill);

			setupIntradayStaffingForSkill(notUnderStaffingSkill, 10d, 15d);
			setupIntradayStaffingForSkill(criticalUnderStaffingSkill, 10d, 6d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
		public void ShouldDenyWhenOnlyUnderStaffingButNoCriticalSkillAndToggle74944IsOn()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new Interfaces.Domain.MinMax<int>(0, 24)
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var notUnderStaffingSkill = createSkill("notUnderStaffingSkill", null, timeZone);
			var underStaffedButNotCriticalSkill = createSkill("criticalUnderStaffingSkill", null, timeZone);

			var personSkill1 = createPersonSkill(activity1, notUnderStaffingSkill);
			var personSkill2 = createPersonSkill(activity2, underStaffedButNotCriticalSkill);

			setupIntradayStaffingForSkill(notUnderStaffingSkill, 10d, 20d);
			setupIntradayStaffingForSkill(underStaffedButNotCriticalSkill, 10d, 15d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
		public void ShouldApproveWhenRequestPeriodCrossTwoOpenPeriodsAndStartTimeIsFromTheWholePoint()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			var emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 24),
				SkillType = emailSkillType
			});

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var underStaffingSkill1 = createSkill("criticalUnderStaffingSkill 1", null, timeZone);
			underStaffingSkill1.SkillType = emailSkillType;

			var underStaffingSkill2 = createSkill("criticalUnderStaffingSkill 2", null, timeZone);
			underStaffingSkill2.SkillType = emailSkillType;

			var personSkill1 = createPersonSkill(activity1, underStaffingSkill1);
			var personSkill2 = createPersonSkill(activity2, underStaffingSkill2);

			Now.Is(new DateTime(2017, 07, 17, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(Now.UtcDateTime());
			var firstPeriod = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);
			var secondPeriod = new DateTimePeriod(2017, 7, 17, 12, 2017, 7, 17, 13);

			setupIntradayStaffingForSkill(underStaffingSkill1, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = secondPeriod},
			});

			setupIntradayStaffingForSkill(underStaffingSkill2, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = secondPeriod},
			});

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 2);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();

			var overtimeActivities = getOvertimeActivities(date);

			overtimeActivities.Count().Should().Be(2);
			var firstOvertimeActivity = overtimeActivities[0];
			firstOvertimeActivity.Payload.Should().Be(activity1);
			firstOvertimeActivity.Period.StartDateTime.Should().Be(new DateTime(2017, 7, 17, 11, 00, 00, DateTimeKind.Utc));
			firstOvertimeActivity.Period.EndDateTime.Should().Be(new DateTime(2017, 7, 17, 12, 00, 00, DateTimeKind.Utc));

			var secondOvertimeActivity = overtimeActivities[1];
			secondOvertimeActivity.Payload.Should().Be(activity2);
			secondOvertimeActivity.Period.StartDateTime.Should().Be(new DateTime(2017, 7, 17, 12, 00, 00, DateTimeKind.Utc));
			secondOvertimeActivity.Period.EndDateTime.Should().Be(new DateTime(2017, 7, 17, 13, 00, 00, DateTimeKind.Utc));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
		public void ShouldApproveWhenRequestPeriodCrossTwoOpenPeriodsAndStartTimeIsBetweenThePeriodIntervalStartAndEndTime()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			var emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 24),
				SkillType = emailSkillType
			});

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var underStaffingSkill1 = createSkill("criticalUnderStaffingSkill 1", null, timeZone);
			underStaffingSkill1.SkillType = emailSkillType;

			var underStaffingSkill2 = createSkill("criticalUnderStaffingSkill 2", null, timeZone);
			underStaffingSkill2.SkillType = emailSkillType;

			var personSkill1 = createPersonSkill(activity1, underStaffingSkill1);
			var personSkill2 = createPersonSkill(activity2, underStaffingSkill2);

			Now.Is(new DateTime(2017, 07, 17, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(Now.UtcDateTime());
			var firstPeriod = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12).ChangeEndTime(-TimeSpan.FromMinutes(30));
			var secondPeriod = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12).ChangeStartTime(TimeSpan.FromMinutes(30));

			setupIntradayStaffingForSkill(underStaffingSkill1, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = secondPeriod},
			});

			setupIntradayStaffingForSkill(underStaffingSkill2, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = secondPeriod},
			});

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequestInMinutes(TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(10)), 50);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();

			var overtimeActivities = getOvertimeActivities(date);

			overtimeActivities.Count().Should().Be(2);
			var firstOvertimeActivity = overtimeActivities[0];
			firstOvertimeActivity.Payload.Should().Be(activity1);
			firstOvertimeActivity.Period.StartDateTime.Should().Be(new DateTime(2017, 7, 17, 11, 10, 00, DateTimeKind.Utc));
			firstOvertimeActivity.Period.EndDateTime.Should().Be(new DateTime(2017, 7, 17, 11, 30, 00, DateTimeKind.Utc));

			var secondOvertimeActivity = overtimeActivities[1];
			secondOvertimeActivity.Payload.Should().Be(activity2);
			secondOvertimeActivity.Period.StartDateTime.Should().Be(new DateTime(2017, 7, 17, 11, 30, 00, DateTimeKind.Utc));
			secondOvertimeActivity.Period.EndDateTime.Should().Be(new DateTime(2017, 7, 17, 12, 00, 00, DateTimeKind.Utc));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
		public void ShouldApproveWhenRequestPeriodCrossTwoOpenPeriodsAndEndTimeIsBetweenThePeriodIntervalStartAndEndTime()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			var emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 24),
				SkillType = emailSkillType
			});

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var underStaffingSkill1 = createSkill("criticalUnderStaffingSkill 1", null, timeZone);
			underStaffingSkill1.SkillType = emailSkillType;

			var underStaffingSkill2 = createSkill("criticalUnderStaffingSkill 2", null, timeZone);
			underStaffingSkill2.SkillType = emailSkillType;

			var personSkill1 = createPersonSkill(activity1, underStaffingSkill1);
			var personSkill2 = createPersonSkill(activity2, underStaffingSkill2);

			Now.Is(new DateTime(2017, 07, 17, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(Now.UtcDateTime());
			var firstPeriod = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);
			var secondPeriod = new DateTimePeriod(2017, 7, 17, 12, 2017, 7, 17, 13);

			setupIntradayStaffingForSkill(underStaffingSkill1, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = secondPeriod},
			});

			setupIntradayStaffingForSkill(underStaffingSkill2, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = secondPeriod},
			});

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequestInMinutes(TimeSpan.FromHours(11), 110);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();

			var overtimeActivities = getOvertimeActivities(date);

			overtimeActivities.Count().Should().Be(2);
			var firstOvertimeActivity = overtimeActivities[0];
			firstOvertimeActivity.Payload.Should().Be(activity1);
			firstOvertimeActivity.Period.StartDateTime.Should().Be(new DateTime(2017, 7, 17, 11, 00, 00, DateTimeKind.Utc));
			firstOvertimeActivity.Period.EndDateTime.Should().Be(new DateTime(2017, 7, 17, 12, 00, 00, DateTimeKind.Utc));

			var secondOvertimeActivity = overtimeActivities[1];
			secondOvertimeActivity.Payload.Should().Be(activity2);
			secondOvertimeActivity.Period.StartDateTime.Should().Be(new DateTime(2017, 7, 17, 12, 00, 00, DateTimeKind.Utc));
			secondOvertimeActivity.Period.EndDateTime.Should().Be(new DateTime(2017, 7, 17, 12, 50, 00, DateTimeKind.Utc));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
		public void ShouldDenyWhenRequestPeriodCrossTwoOpenPeriodsAndStaffingDataIsPartlyMissing()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			var emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 24),
				SkillType = emailSkillType
			});

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var underStaffingSkill1 = createSkill("criticalUnderStaffingSkill 1", null, timeZone);
			underStaffingSkill1.SkillType = emailSkillType;

			var underStaffingSkill2 = createSkill("criticalUnderStaffingSkill 2", null, timeZone);
			underStaffingSkill2.SkillType = emailSkillType;

			var personSkill1 = createPersonSkill(activity1, underStaffingSkill1);
			var personSkill2 = createPersonSkill(activity2, underStaffingSkill2);

			Now.Is(new DateTime(2017, 07, 17, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(Now.UtcDateTime());
			var firstPeriod = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);
			var secondPeriod = new DateTimePeriod(2017, 7, 17, 12, 2017, 7, 17, 13);

			setupIntradayStaffingForSkill(underStaffingSkill1, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = secondPeriod},
			});

			setupIntradayStaffingForSkill(underStaffingSkill2, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = firstPeriod}
			});

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequestInMinutes(TimeSpan.FromHours(11), 110);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(Resources.NoUnderStaffingSkill);
		}

		private OvertimeShiftLayer[] getOvertimeActivities(DateOnly date)
		{
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), date.ToDateOnlyPeriod(), Scenario.Current());
			var overtimeActivities = scheduleDictionary.SchedulesForDay(date).First().PersonAssignment().OvertimeActivities().ToArray();

			return overtimeActivities;
		}
	}
}