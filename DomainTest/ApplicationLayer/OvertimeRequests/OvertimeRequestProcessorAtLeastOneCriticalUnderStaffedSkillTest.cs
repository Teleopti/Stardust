using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
		public void ShouldDenyWhenOnlyUnderStaffingButNoCriticalSkillAndToggle74944IsOn()
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
		public void ShouldApproveWhenRequestPeriodCrossTwoOpenPeriodsAndStartTimeIsFromTheWholePoint()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 24)
			});

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var underStaffingSkill1 = createSkill("criticalUnderStaffingSkill 1", null, timeZone);
			underStaffingSkill1.SkillType = _emailSkillType;

			var underStaffingSkill2 = createSkill("criticalUnderStaffingSkill 2", null, timeZone);
			underStaffingSkill2.SkillType = _emailSkillType;

			var personSkill1 = createPersonSkill(activity1, underStaffingSkill1);
			var personSkill2 = createPersonSkill(activity2, underStaffingSkill2);

			Now.Is(new DateTime(2017, 07, 17, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(Now.UtcDateTime());
			var firstPeriod = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);
			var secondPeriod = new DateTimePeriod(2017, 7, 17, 12, 2017, 7, 17, 13);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkill1, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = secondPeriod},
			}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkill2, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = secondPeriod},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 2);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();

			var overtimeActivities = getOvertimeActivities(date);

			overtimeActivities.Length.Should().Be(2);
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
		public void ShouldApproveWhenRequestPeriodCrossTwoOpenPeriodsAndStartTimeIsBetweenThePeriodIntervalStartAndEndTime()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 24)
			});

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var underStaffingSkill1 = createSkill("criticalUnderStaffingSkill 1", null, timeZone);
			underStaffingSkill1.SkillType = _emailSkillType;

			var underStaffingSkill2 = createSkill("criticalUnderStaffingSkill 2", null, timeZone);
			underStaffingSkill2.SkillType = _emailSkillType;

			var personSkill1 = createPersonSkill(activity1, underStaffingSkill1);
			var personSkill2 = createPersonSkill(activity2, underStaffingSkill2);

			Now.Is(new DateTime(2017, 07, 17, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(Now.UtcDateTime());
			var firstPeriod = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12).ChangeEndTime(-TimeSpan.FromMinutes(30));
			var secondPeriod = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12).ChangeStartTime(TimeSpan.FromMinutes(30));

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkill1, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = secondPeriod},
			}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkill2, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = secondPeriod},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequestInMinutes(TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(10)), 50);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();

			var overtimeActivities = getOvertimeActivities(date);

			overtimeActivities.Length.Should().Be(2);
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
		public void ShouldApproveWhenRequestPeriodCrossTwoOpenPeriodsAndEndTimeIsBetweenThePeriodIntervalStartAndEndTime()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 24)
			});

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var underStaffingSkill1 = createSkill("criticalUnderStaffingSkill 1", null, timeZone);
			underStaffingSkill1.SkillType = _emailSkillType;

			var underStaffingSkill2 = createSkill("criticalUnderStaffingSkill 2", null, timeZone);
			underStaffingSkill2.SkillType = _emailSkillType;

			var personSkill1 = createPersonSkill(activity1, underStaffingSkill1);
			var personSkill2 = createPersonSkill(activity2, underStaffingSkill2);

			Now.Is(new DateTime(2017, 07, 17, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(Now.UtcDateTime());
			var firstPeriod = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);
			var secondPeriod = new DateTimePeriod(2017, 7, 17, 12, 2017, 7, 17, 13);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkill1, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = secondPeriod},
			}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkill2, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = secondPeriod},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequestInMinutes(TimeSpan.FromHours(11), 110);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();

			var overtimeActivities = getOvertimeActivities(date);

			overtimeActivities.Length.Should().Be(2);
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
		public void ShouldApproveWhenThereIsAtLeastOneSkillCriticalUnderstaffedWithAutoGrantSetToYes()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType, _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 7),
				OrderIndex = 1
			});

			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 2
			});

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var phoneActivity = createActivity("phone activity");
			var emailActivity = createActivity("email activity");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var underStaffingSkillPhone = createSkill("criticalUnderStaffingSkillPhone", null, timeZone);
			underStaffingSkillPhone.SkillType = _phoneSkillType;

			var underStaffingSkillEmail = createSkill("criticalUnderStaffingSkillEmail", null, timeZone);
			underStaffingSkillEmail.SkillType = _emailSkillType;
			underStaffingSkillEmail.DefaultResolution = 60;

			var personSkillPhone = createPersonSkill(phoneActivity, underStaffingSkillPhone);
			var personSkillEmail = createPersonSkill(emailActivity, underStaffingSkillEmail);

			Now.Is(new DateTime(2017, 07, 17, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(Now.UtcDateTime());
			var period = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkillPhone, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = period}
			}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkillEmail, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = period}
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkillPhone, personSkillEmail);

			var personRequest = createOvertimeRequestInMinutes(TimeSpan.FromHours(11), 60);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();

			var overtimeActivities = getOvertimeActivities(date);

			overtimeActivities.Length.Should().Be(1);
			var firstOvertimeActivity = overtimeActivities[0];
			firstOvertimeActivity.Payload.Should().Be(phoneActivity);
			firstOvertimeActivity.Period.StartDateTime.Should().Be(new DateTime(2017, 7, 17, 11, 00, 00, DateTimeKind.Utc));
			firstOvertimeActivity.Period.EndDateTime.Should().Be(new DateTime(2017, 7, 17, 12, 00, 00, DateTimeKind.Utc));
		}

		[Test]
		public void ShouldDenyWhenRequestPeriodCrossTwoOpenPeriodsAndStaffingDataIsPartlyMissing()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 24)
			});

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var underStaffingSkill1 = createSkill("criticalUnderStaffingSkill 1", null, timeZone);
			underStaffingSkill1.SkillType = _emailSkillType;

			var underStaffingSkill2 = createSkill("criticalUnderStaffingSkill 2", null, timeZone);
			underStaffingSkill2.SkillType = _emailSkillType;

			var personSkill1 = createPersonSkill(activity1, underStaffingSkill1);
			var personSkill2 = createPersonSkill(activity2, underStaffingSkill2);

			Now.Is(new DateTime(2017, 07, 17, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(Now.UtcDateTime());
			var firstPeriod = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);
			var secondPeriod = new DateTimePeriod(2017, 7, 17, 12, 2017, 7, 17, 13);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkill1, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = secondPeriod},
			}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkill2, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = firstPeriod}
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequestInMinutes(TimeSpan.FromHours(11), 110);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(Resources.NoUnderStaffingSkill);
		}

		[Test]
		public void ShouldNotCreateOvertimeActivityOutsideOfRequestPeriod()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 7),
				OrderIndex = 1
			});

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var phoneActivity = createActivity("phone activity");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var underStaffingSkillPhone = createSkill("criticalUnderStaffingSkillPhone", null, timeZone);
			underStaffingSkillPhone.DefaultResolution = 30;
			underStaffingSkillPhone.SkillType = _phoneSkillType;

			var personSkillPhone = createPersonSkill(phoneActivity, underStaffingSkillPhone);

			Now.Is(new DateTime(2017, 07, 17, 10, 00, 00, DateTimeKind.Utc));
			var date = new DateOnly(Now.UtcDateTime());
			var period = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 13);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkillPhone, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = period}
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkillPhone);

			var personRequest = createOvertimeRequestInMinutes(TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(15)), 60);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();

			var overtimeActivities = getOvertimeActivities(date);

			overtimeActivities.Length.Should().Be(1);
			var firstOvertimeActivity = overtimeActivities[0];
			firstOvertimeActivity.Payload.Should().Be(phoneActivity);
			firstOvertimeActivity.Period.StartDateTime.Should().Be(new DateTime(2017, 7, 17, 11, 15, 00, DateTimeKind.Utc));
			firstOvertimeActivity.Period.EndDateTime.Should().Be(new DateTime(2017, 7, 17, 12, 15, 00, DateTimeKind.Utc));
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