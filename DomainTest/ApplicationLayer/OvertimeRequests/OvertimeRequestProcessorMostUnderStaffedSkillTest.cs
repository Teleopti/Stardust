using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseActivityOfMostUnderstaffedSkillAsOvertimeActivity()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffedActivity = createActivity("activity1");
			var moreCriticalUnderStaffedActivity = createActivity("activity2");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var criticalUnderStaffedSkill1 = createSkill("criticalUnderStaffedSkill1", null, timeZone);
			var criticalUnderStaffedSkill2 = createSkill("criticalUnderStaffedSkill2", null, timeZone);

			var personSkill1 = createPersonSkill(criticalUnderStaffedActivity, criticalUnderStaffedSkill1);
			var personSkill2 = createPersonSkill(moreCriticalUnderStaffedActivity, criticalUnderStaffedSkill2);

			setupIntradayStaffingForSkill(criticalUnderStaffedSkill1, 10d, 2d);
			setupIntradayStaffingForSkill(criticalUnderStaffedSkill2, 10d, 1d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(moreCriticalUnderStaffedActivity);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseFirstActivityOfSameUnderstaffedLevelSkillsAsOvertimeActivity()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffedActivity1 = createActivity("activity1");
			var criticalUnderStaffedActivity2 = createActivity("activity2");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var criticalUnderStaffedSkill1 = createSkill("criticalUnderStaffedSkill1", null, timeZone);
			var criticalUnderStaffedSkill2 = createSkill("criticalUnderStaffedSkill2", null, timeZone);

			var personSkill1 = createPersonSkill(criticalUnderStaffedActivity1, criticalUnderStaffedSkill1);
			var personSkill2 = createPersonSkill(criticalUnderStaffedActivity2, criticalUnderStaffedSkill2);

			setupIntradayStaffingForSkill(criticalUnderStaffedSkill1, 10d, 2d);
			setupIntradayStaffingForSkill(criticalUnderStaffedSkill2, 10d, 2d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(criticalUnderStaffedActivity1);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseActivityOfFirstMostUnderstaffedSkillAsOvertimeActivity()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffedActivity = createActivity("criticalUnderStaffedActivity");
			var moreCriticalUnderStaffedActivity1 = createActivity("moreCriticalUnderStaffedActivity1");
			var moreCriticalUnderStaffedActivity2 = createActivity("moreCriticalUnderStaffedActivity2");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var criticalUnderStaffedSkill = createSkill("criticalUnderStaffedSkill", null, timeZone);
			var moreCriticalUnderStaffedSkill1 = createSkill("moreCriticalUnderStaffedSkill1", null, timeZone);
			var moreCriticalUnderStaffedSkill2 = createSkill("moreCriticalUnderStaffedSkill2", null, timeZone);

			var personSkill1 = createPersonSkill(criticalUnderStaffedActivity, criticalUnderStaffedSkill);
			var personSkill2 = createPersonSkill(moreCriticalUnderStaffedActivity1, moreCriticalUnderStaffedSkill1);
			var personSkill3 = createPersonSkill(moreCriticalUnderStaffedActivity2, moreCriticalUnderStaffedSkill2);

			setupIntradayStaffingForSkill(criticalUnderStaffedSkill, 10d, 2d);
			setupIntradayStaffingForSkill(moreCriticalUnderStaffedSkill1, 10d, 1d);
			setupIntradayStaffingForSkill(moreCriticalUnderStaffedSkill2, 10d, 1d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2, personSkill3);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()]
				.ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(moreCriticalUnderStaffedActivity1);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseActivityOfMostUnderstaffedSkillAsOvertimeActivityWithinRequestPeriod()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffedActivity1 = createActivity("activity1");
			var criticalUnderStaffedActivity2 = createActivity("activity2");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var criticalUnderStaffedSkill1 = createSkill("criticalUnderStaffedSkill1", null, timeZone);
			var criticalUnderStaffedSkill2 = createSkill("criticalUnderStaffedSkill2", null, timeZone);

			var date = new DateOnly(2017, 7, 17);
			var personSkill1 = createPersonSkill(criticalUnderStaffedActivity1, criticalUnderStaffedSkill1);
			var personSkill2 = createPersonSkill(criticalUnderStaffedActivity2, criticalUnderStaffedSkill2);

			var firstPeriod = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);
			var secondPeriod = new DateTimePeriod(2017, 7, 17, 12, 2017, 7, 17, 13);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffedSkill1, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = secondPeriod},
			},timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffedSkill2, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 5d, Period = secondPeriod},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()]
				.ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(criticalUnderStaffedActivity2);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseEmailActivityAsOvertimeActivityWhenEmailSKillIsMoreCriticalUnderStaffed()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var phoneActivity = createActivity("phoneActivity");
			var emailActivity = createActivity("emailActivity");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var criticalUnderStaffedPhoneSkill = createSkill("criticalUnderStaffedPhoneSkill", null, timeZone);
			criticalUnderStaffedPhoneSkill.SkillType = _phoneSkillType;
			var moreCriticalUnderStaffedPhoneSkill = createSkill("moreCriticalUnderStaffedPhoneSkill", null, timeZone);
			moreCriticalUnderStaffedPhoneSkill.SkillType = _phoneSkillType;
			var mostCriticalUnderStaffedEmailSkill = createSkill("mostCriticalUnderStaffedEmailSkill", null, timeZone);
			mostCriticalUnderStaffedEmailSkill.SkillType = _phoneSkillType;
			mostCriticalUnderStaffedEmailSkill.DefaultResolution = 60;

			var personSkill1 = createPersonSkill(phoneActivity, criticalUnderStaffedPhoneSkill);
			var personSkill2 = createPersonSkill(phoneActivity, moreCriticalUnderStaffedPhoneSkill);
			var personSkill3 = createPersonSkill(emailActivity, mostCriticalUnderStaffedEmailSkill);

			var date = new DateOnly(2017, 7, 17);
			var period = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffedPhoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = period},
			}, timeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(moreCriticalUnderStaffedPhoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = period},
			}, timeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(mostCriticalUnderStaffedEmailSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 0d, Period = period},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2, personSkill3);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(emailActivity);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUsePhoneActivityAsOvertimeActivityWhenPhoneSKillIsMoreCriticalUnderStaffed()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var phoneActivity = createActivity("phoneActivity");
			var emailActivity = createActivity("emailActivity");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var criticalUnderStaffedPhoneSkill = createSkill("criticalUnderStaffedPhoneSkill", null, timeZone);
			criticalUnderStaffedPhoneSkill.SkillType = _phoneSkillType;

			var moreCriticalUnderStaffedEmailSkill = createSkill("moreCriticalUnderStaffedEmailSkill", null, timeZone);
			moreCriticalUnderStaffedEmailSkill.SkillType = _phoneSkillType;
			moreCriticalUnderStaffedEmailSkill.DefaultResolution = 60;

			var mostCriticalUnderStaffedPhoneSkill = createSkill("mostCriticalUnderStaffedPhoneSkill", null, timeZone);
			mostCriticalUnderStaffedPhoneSkill.SkillType = _phoneSkillType;

			var personSkill1 = createPersonSkill(phoneActivity, criticalUnderStaffedPhoneSkill);
			var personSkill2 = createPersonSkill(emailActivity, moreCriticalUnderStaffedEmailSkill);
			var personSkill3 = createPersonSkill(phoneActivity, mostCriticalUnderStaffedPhoneSkill);

			var date = new DateOnly(2017, 7, 17);
			var period = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 13);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffedPhoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = period},
			},timeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(moreCriticalUnderStaffedEmailSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = period},
			}, timeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(mostCriticalUnderStaffedPhoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 0d, Period = period},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2, personSkill3);

			var personRequest = createOvertimeRequestInMinutes(11, 75);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(phoneActivity);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseEmailActivityAsOvertimeActivityWhenEmailSKillIsMoreCriticalUnderStaffedIn30Minutes()
		{
			setupPerson();
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			UserTimeZone.Is(TimeZoneInfoFactory.StockholmTimeZoneInfo());

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var phoneActivity = createActivity("phoneActivity");
			var emailActivity = createActivity("emailActivity");

			var skillTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var criticalUnderStaffedPhoneSkill = createSkill("criticalUnderStaffedPhoneSkill", null, skillTimeZone);
			criticalUnderStaffedPhoneSkill.SkillType = _phoneSkillType;
			var moreCriticalUnderStaffedPhoneSkill = createSkill("moreCriticalUnderStaffedPhoneSkill", null, skillTimeZone);
			moreCriticalUnderStaffedPhoneSkill.SkillType = _phoneSkillType;
			var mostCriticalUnderStaffedEmailSkill = createSkill("mostCriticalUnderStaffedEmailSkill", null, skillTimeZone);
			mostCriticalUnderStaffedEmailSkill.SkillType = _phoneSkillType;
			mostCriticalUnderStaffedEmailSkill.DefaultResolution = 60;

			var personSkill1 = createPersonSkill(phoneActivity, criticalUnderStaffedPhoneSkill);
			var personSkill2 = createPersonSkill(phoneActivity, moreCriticalUnderStaffedPhoneSkill);
			var personSkill3 = createPersonSkill(emailActivity, mostCriticalUnderStaffedEmailSkill);

			var date = new DateOnly(2017, 7, 17);
			var period = new DateTimePeriod(
				TimeZoneHelper.ConvertToUtc(new DateTime(2017, 7, 17, 11, 00, 00), skillTimeZone), 
				TimeZoneHelper.ConvertToUtc(new DateTime(2017, 7, 17, 12, 00, 00), skillTimeZone));

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffedPhoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = period},
			}, skillTimeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(moreCriticalUnderStaffedPhoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = period},
			}, skillTimeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(mostCriticalUnderStaffedEmailSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 0d, Period = period},
			}, skillTimeZone);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2, personSkill3);

			var personRequest = createOvertimeRequestInMinutes(11, 30, skillTimeZone);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(emailActivity);
		}

		[Test]
		public void ShouldUseEmailActivityAsOvertimeActivityWhenEmailSKillIsMoreCriticalUnderStaffedIn30MinutesWhenToggle47853IsOff()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var phoneActivity = createActivity("phoneActivity");
			var emailActivity = createActivity("emailActivity");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var criticalUnderStaffedPhoneSkill = createSkill("criticalUnderStaffedPhoneSkill", null, timeZone);
			criticalUnderStaffedPhoneSkill.SkillType = _phoneSkillType;
			var moreCriticalUnderStaffedPhoneSkill = createSkill("moreCriticalUnderStaffedPhoneSkill", null, timeZone);
			moreCriticalUnderStaffedPhoneSkill.SkillType = _phoneSkillType;
			var mostCriticalUnderStaffedEmailSkill = createSkill("mostCriticalUnderStaffedEmailSkill", null, timeZone);
			mostCriticalUnderStaffedEmailSkill.SkillType = _phoneSkillType;
			mostCriticalUnderStaffedEmailSkill.DefaultResolution = 60;

			var personSkill1 = createPersonSkill(emailActivity, mostCriticalUnderStaffedEmailSkill);
			var personSkill2 = createPersonSkill(phoneActivity, moreCriticalUnderStaffedPhoneSkill);
			var personSkill3 = createPersonSkill(phoneActivity, criticalUnderStaffedPhoneSkill);

			var date = new DateOnly(2017, 7, 17);
			var period = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffedPhoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = period},
			}, timeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(moreCriticalUnderStaffedPhoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = period},
			}, timeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(mostCriticalUnderStaffedEmailSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 0d, Period = period},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2, personSkill3);

			var personRequest = createOvertimeRequestInMinutes(11, 30);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(emailActivity);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldSupportDiffrentSkillTypesWithTwoFullOverlappedPeriods()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13),
				OrderIndex = 1
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var phoneActivity = createActivity("phoneActivity");
			var emailActivity = createActivity("emailActivity");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var moreCriticalUnderStaffedPhoneSkill = createSkill("moreCriticalUnderStaffedPhoneSkill", null, timeZone);
			moreCriticalUnderStaffedPhoneSkill.SkillType = _phoneSkillType;
			var mostCriticalUnderStaffedEmailSkill = createSkill("mostCriticalUnderStaffedEmailSkill", null, timeZone);
			mostCriticalUnderStaffedEmailSkill.SkillType = _emailSkillType;
			mostCriticalUnderStaffedEmailSkill.DefaultResolution = 60;

			var personSkill2 = createPersonSkill(phoneActivity, moreCriticalUnderStaffedPhoneSkill);
			var personSkill3 = createPersonSkill(emailActivity, mostCriticalUnderStaffedEmailSkill);

			var date = new DateOnly(2017, 7, 17);
			var period = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(moreCriticalUnderStaffedPhoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = period},
			}, timeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(mostCriticalUnderStaffedEmailSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 0d, Period = period},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkill2, personSkill3);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(emailActivity);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldSupportDiffrentSkillTypesWithTwoPartialOverlappedPeriods()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			var emailTypeRollingPeriodStartsFromToday = new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13),
				OrderIndex = 0
			};
			workflowControlSet.AddOpenOvertimeRequestPeriod(emailTypeRollingPeriodStartsFromToday);
			var phoneTypeRollingPeriodStartsFromTomorrow = new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(1, 13),
				OrderIndex = 1
			};
			workflowControlSet.AddOpenOvertimeRequestPeriod(phoneTypeRollingPeriodStartsFromTomorrow);
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var phoneActivity = createActivity("phoneActivity");
			var emailActivity = createActivity("emailActivity");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var moreCriticalUnderStaffedPhoneSkill = createSkill("moreCriticalUnderStaffedPhoneSkill", null, timeZone);
			moreCriticalUnderStaffedPhoneSkill.SkillType = _phoneSkillType;
			var mostCriticalUnderStaffedEmailSkill = createSkill("mostCriticalUnderStaffedEmailSkill", null, timeZone);
			mostCriticalUnderStaffedEmailSkill.SkillType = _emailSkillType;
			mostCriticalUnderStaffedEmailSkill.DefaultResolution = 60;

			var personSkill2 = createPersonSkill(phoneActivity, moreCriticalUnderStaffedPhoneSkill);
			var personSkill3 = createPersonSkill(emailActivity, mostCriticalUnderStaffedEmailSkill);

			var tomorrowDate = Now.UtcDateTime().Date.AddDays(1);
			var tomorrow = new DateOnly(tomorrowDate);
			var period = new DateTimePeriod(tomorrowDate.AddHours(11), tomorrowDate.AddHours(12));

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(moreCriticalUnderStaffedPhoneSkill, tomorrow, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = period},
			}, timeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(mostCriticalUnderStaffedEmailSkill, tomorrow, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 0d, Period = period},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkill2, personSkill3);

			var personRequest = createOvertimeRequest(tomorrowDate.AddHours(11), 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(emailActivity);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldOnlyUseOpenPeriodWithPeriodFullyMatchedToCheckSkillType()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			var emailTypeRollingPeriodStartsFromToday = new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13),
				OrderIndex = 0
			};
			workflowControlSet.AddOpenOvertimeRequestPeriod(emailTypeRollingPeriodStartsFromToday);
			var phoneTypeRollingPeriodStartsFromTomorrow = new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(1, 13),
				OrderIndex = 1
			};
			workflowControlSet.AddOpenOvertimeRequestPeriod(phoneTypeRollingPeriodStartsFromTomorrow);
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var phoneActivity = createActivity("phoneActivity");
			var emailActivity = createActivity("emailActivity");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var moreCriticalUnderStaffedPhoneSkill = createSkill("moreCriticalUnderStaffedPhoneSkill", null, timeZone);
			moreCriticalUnderStaffedPhoneSkill.SkillType = _phoneSkillType;
			var mostCriticalUnderStaffedEmailSkill = createSkill("mostCriticalUnderStaffedEmailSkill", null, timeZone);
			mostCriticalUnderStaffedEmailSkill.SkillType = _emailSkillType;
			mostCriticalUnderStaffedEmailSkill.DefaultResolution = 60;

			var personSkill2 = createPersonSkill(phoneActivity, moreCriticalUnderStaffedPhoneSkill);
			var personSkill3 = createPersonSkill(emailActivity, mostCriticalUnderStaffedEmailSkill);

			var nowDate = Now.UtcDateTime().Date;
			var date = new DateOnly(nowDate);
			var period = new DateTimePeriod(nowDate.AddHours(11), nowDate.AddHours(12));

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(moreCriticalUnderStaffedPhoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = period},
			}, timeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(mostCriticalUnderStaffedEmailSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 0d, Period = period},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personSkill2, personSkill3);

			var personRequest = createOvertimeRequest(nowDate.AddHours(11), 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(emailActivity);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldApproveRequestInLessThan15MinutesWithToggle47853On()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var personRequest = createOvertimeRequestInMinutes(18, 10);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldApproveRequestWithPeriodEquals15MinutesWithToggle47853On()
		{
			setupPerson(8, 21);

			var skill = setupPersonSkill();
			skill.DefaultResolution = 60;
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var period = new DateTimePeriod(2017, 7, 17, 18, 2017, 7, 17, 19);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, new DateOnly(2017, 7, 17), new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = period},
			}, timeZone);

			var personRequest = createOvertimeRequestInMinutes(18, 15);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldApproveRequestWithPeriodStartTimeEquals45MinutesWithToggle47853On()
		{
			setupPerson(8, 21);

			var skill = setupPersonSkill();
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			skill.DefaultResolution = 60;
			var period = new DateTimePeriod(2017, 7, 17, 18, 2017, 7, 17, 19);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, new DateOnly(2017, 7, 17), new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = period},
			}, timeZone);

			var personRequest = createOvertimeRequestInMinutes(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(45)), 15);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseOvertimeRequestPeriodWithMostCriticalOfMatchedUnderStaffedSkillType()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(0, 13),
				OrderIndex = 1
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var phoneActivity = createActivity("phoneActivity");
			var emailActivity = createActivity("emailActivity");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var moreCriticalUnderStaffedEmailSkill = createSkill("moreCriticalUnderStaffedPhoneSkill", null, timeZone);
			moreCriticalUnderStaffedEmailSkill.SkillType = _phoneSkillType;
			var mostCriticalUnderStaffedPhoneSkill = createSkill("mostCriticalUnderStaffedEmailSkill", null, timeZone);
			mostCriticalUnderStaffedPhoneSkill.SkillType = _emailSkillType;
			mostCriticalUnderStaffedPhoneSkill.DefaultResolution = 60;

			var personEmailSkill = createPersonSkill(phoneActivity, moreCriticalUnderStaffedEmailSkill);
			var personPhoneSkill = createPersonSkill(emailActivity, mostCriticalUnderStaffedPhoneSkill);

			var date = new DateOnly(2017, 7, 17);
			var period = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(moreCriticalUnderStaffedEmailSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = period},
			}, timeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(mostCriticalUnderStaffedPhoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 0d, Period = period},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personEmailSkill, personPhoneSkill);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget(27).Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(emailActivity);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseOvertimeRequestPeriodWithHigherPriorityOfMatchedSkillType()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 1),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(0, 1),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(1, 2),
				OrderIndex = 1
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var emailActivity = createActivity("emailActivity");
			var phoneActivity = createActivity("phoneActivity");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var criticalUnderStaffedPhoneSkill = createSkill("criticalUnderStaffedPhoneSkill", null, timeZone);
			criticalUnderStaffedPhoneSkill.SkillType = _phoneSkillType;

			var mostCriticalUnderStaffedEmailSkill = createSkill("mostCriticalUnderStaffedEmailSkill", null, timeZone);
			mostCriticalUnderStaffedEmailSkill.SkillType = _emailSkillType;
			mostCriticalUnderStaffedEmailSkill.DefaultResolution = 60;

			var personEmailSkill = createPersonSkill(emailActivity, mostCriticalUnderStaffedEmailSkill);
			var personPhoneSkill = createPersonSkill(phoneActivity, criticalUnderStaffedPhoneSkill);

			var date = new DateOnly(2017, 7, 13);
			var period = new DateTimePeriod(2017, 7, 13, 11, 2017, 7, 13, 12);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(criticalUnderStaffedPhoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = period},
			}, timeZone);
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(mostCriticalUnderStaffedEmailSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = period},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personPhoneSkill, personEmailSkill);

			var requestStartTime = new DateTime(2017, 7, 13, 11, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(requestStartTime, 1);

			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldCheckSkillOpenHourWhenApprovingByAdministrator()
		{
			Now.Is(new DateTime(2017, 7, 13, 11, 0, 0, DateTimeKind.Utc));
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 48),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				BetweenDays = new MinMax<int>(0, 48),
				OrderIndex = 1
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var chatActivity = createActivity("chatActivity");
			var phoneActivity = createActivity("phoneActivity");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var days = new Dictionary<DayOfWeek, TimePeriod>();
			for (var i = 1; i < 6; i++)
			{
				days.Add((DayOfWeek)i, new TimePeriod(8, 18));
			}

			var phoneSkill = createSkillWithDifferentOpenHourPeriod("phoneSkill", days, timeZone);
			phoneSkill.SkillType = _phoneSkillType;

			days.Clear();
			for (var i = 0; i < 7; i++)
			{
				days.Add((DayOfWeek)i, new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1)));
			}
			var chatSkill = createSkillWithDifferentOpenHourPeriod("chatSkill", days, timeZone);
			chatSkill.SkillType = _chatSkillType;

			var personChatSkill = createPersonSkill(chatActivity, chatSkill);
			var personPhoneSkill = createPersonSkill(phoneActivity, phoneSkill);

			var date = new DateOnly(2017, 7, 15);
			var period = new DateTimePeriod(2017, 7, 15, 11, 2017, 7, 15, 12);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(chatSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = period},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personPhoneSkill, personChatSkill);

			var requestStartTime = new DateTime(2017, 7, 15, 11, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(requestStartTime, 1);

			getTarget().Process(personRequest);

			personRequest.IsPending.Should().Be.True();

			personRequest.Approve(RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null), new PersonRequestAuthorizationCheckerForTest());

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(chatActivity);

		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldFailIfNoSkillIsMatchedWithSkillOpenHourWhenApprovingByAdministrator()
		{
			Now.Is(new DateTime(2017, 7, 13, 11, 0, 0, DateTimeKind.Utc));
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				BetweenDays = new MinMax<int>(0, 48),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				BetweenDays = new MinMax<int>(0, 48),
				OrderIndex = 1
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var chatActivity = createActivity("chatActivity");
			var phoneActivity = createActivity("phoneActivity");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var phoneSkill = createSkill("phoneSkill", new TimePeriod(8, 18), timeZone);
			phoneSkill.SkillType = _phoneSkillType;

			var chatSkill = createSkill("chatSkill", new TimePeriod(8, 18), timeZone);
			chatSkill.SkillType = _chatSkillType;

			var personChatSkill = createPersonSkill(chatActivity, chatSkill);
			var personPhoneSkill = createPersonSkill(phoneActivity, phoneSkill);

			var date = new DateOnly(2017, 7, 15);
			var period = new DateTimePeriod(2017, 7, 15, 11, 2017, 7, 15, 12);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(phoneSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = period},
			}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(chatSkill, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = period},
			}, timeZone);

			addPersonSkillsToPersonPeriod(personPhoneSkill, personChatSkill);

			var requestStartTime = new DateTime(2017, 7, 15, 11, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(requestStartTime, 1);

			getTarget().Process(personRequest);

			personRequest.IsPending.Should().Be.True();

			clearOpenHours(phoneSkill);
			clearOpenHours(chatSkill);

			var responses = personRequest.Approve(RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null), new PersonRequestAuthorizationCheckerForTest());

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			responses.Count.Should().Be(1);
			responses[0].Message.Should().Be(Resources.PeriodIsOutOfSkillOpenHours);
			personRequest.IsPending.Should().Be.True();
			personAssignment.Should().Be(null);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldCheckContractRuleBasedOnAvailableSkillType()
		{
			setupPerson(8, 21);

			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(60), TimeSpan.FromHours(10), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Deny,
				BetweenDays = new MinMax<int>(0, 7)
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeValidationHandleType.Pending,
				BetweenDays = new MinMax<int>(0, 48)
			});
			person.WorkflowControlSet = workflowControlSet;

			var channelSupportActivity = createActivity("activity1");
			var webChatActivity = createActivity("activity2");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var channelSupportSkill = createSkill("channel support", new TimePeriod(8, 18), timeZone);
			channelSupportSkill.SkillType = _phoneSkillType;
			var webChatSkill = createSkill("web chat", new TimePeriod(0, 24), timeZone);
			webChatSkill.SkillType = _chatSkillType;

			var channelSupportPersonSkill = createPersonSkill(channelSupportActivity, channelSupportSkill);
			var webChatPersonSkill = createPersonSkill(webChatActivity, webChatSkill);

			setupIntradayStaffingForSkill(channelSupportSkill, 10d, 8d);
			setupIntradayStaffingForSkill(webChatSkill, 10d, 8d);

			addPersonSkillsToPersonPeriod(channelSupportPersonSkill, webChatPersonSkill);

			var scheduleDataOne = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 13, 8, 2017, 7, 13, 20));
			var scheduleDataTwo = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 14, 8, 2017, 7, 14, 16));
			ScheduleStorage.Add(scheduleDataOne);
			ScheduleStorage.Add(scheduleDataTwo);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 20, 0, 0, DateTimeKind.Utc), 5);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should().Be(string.Format(Resources.BusinessRuleNightlyRestRuleErrorMessage, "10:00", "7/13/2017", "7/14/2017", "7:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.NewNightlyRestRule);
		}

		private static void clearOpenHours(ISkill skill)
		{
			foreach (var workload in skill.WorkloadCollection)
			{
				workload.TemplateWeekCollection.ForEach(x => x.Value.ChangeOpenHours(new List<TimePeriod>()));
			}
		}
	}
}