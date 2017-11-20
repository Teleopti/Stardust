﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	[DomainTest]
	[TestFixture]
	[SetCulture("en-US")]
	[SetUICulture("en-US")]
	public class OvertimeRequestProcessorTest : ISetup
	{
		public IOvertimeRequestProcessor Target;
		public IPersonRequestRepository PersonRequestRepository;
		public FakeCurrentScenario_DoNotUse CurrentScenario;
		public FakeLoggedOnUser LoggedOnUser;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public const int MinimumApprovalThresholdTimeInMinutes = 15;
		public INow Now;
		public FakeRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public IScheduleStorage ScheduleStorage;
		public FakeSkillRepository SkillRepository;
		public FakeSkillCombinationResourceRepository CombinationRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public ICurrentScenario Scenario;
		public ISchedulingResultStateHolder SchedulingResultStateHolder;

		private readonly TimePeriod _defaultOpenPeriod = new TimePeriod(8, 00, 21, 00);
		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);
		private TimeSpan[] _intervals;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<OvertimeRequestProcessor>().For<IOvertimeRequestProcessor>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentScenario_DoNotUse>().For<ICurrentScenario>();
			system.UseTestDouble<DoNothingScheduleDayChangeCallBack>().For<IScheduleDayChangeCallback>();
			system.UseTestDouble<SiteOpenHoursSpecification>().For<ISiteOpenHoursSpecification>();
			system.UseTestDouble<FakeRequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();
			system.UseTestDouble<ScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble(new ThisIsNow(new DateTime(2017, 07, 12, 10, 00, 00, DateTimeKind.Utc))).For<INow>();
			_intervals = createIntervals();
		}

		[Test]
		public void ShouldApproveUseIntradyShrinkage()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var personRequest = createOvertimeRequest(18, 1);
			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenThereIsNoUnderStaffingSkillWithShrinkage()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 15d);

			var personRequest = createOvertimeRequest(18, 1);
			Target.Process(personRequest, true);

			personRequest.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldNotLoadStaffingDataAgainWhenValidatedSkillsAreaProvided()
		{
			setupPerson(8, 21);
			var skill = setupPersonSkill();
			setupIntradayStaffingForSkill(skill, 10d, 5d);

			var personRequest = createOvertimeRequest(18, 1);
			RequestApprovalServiceFactory.SetApprovalService(new OvertimeRequestApprovalService(null, null, new FakeCommandDispatcher(), new[] { skill }));

			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApprovedWhenScheduleIsNotInContractTime()
		{
			setupPerson(8, 20);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 18);
			var pa = createMainPersonAssignment(LoggedOnUser.CurrentUser(), period);
			var lunch = ActivityFactory.CreateActivity("lunch").WithId();
			lunch.InContractTime = false;
			pa.AddActivity(lunch, new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12));
			ScheduleStorage.Add(pa);

			var personRequest = createOvertimeRequest(11, 1);
			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldNotApproveWhenAutoGrantoIsOff()
		{
			setupPerson();
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(8, 1);
			Target.Process(personRequest, false);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsPending.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenItsStartTimeIsWithinUpcoming15Mins()
		{
			setupPerson();
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var requestStartTime = Now.UtcDateTime().AddMinutes(MinimumApprovalThresholdTimeInMinutes - 1);
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));
			Target.CheckAndProcessDeny(personRequest);
			Target.Process(personRequest, true);

			var timeZoneInfo = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(string.Format(Resources.OvertimeRequestDenyReasonExpired, TimeZoneHelper.ConvertFromUtc(requestStartTime, timeZoneInfo), MinimumApprovalThresholdTimeInMinutes));
		}

		[Test]
		public void ShouldDenyWhenOutofSiteOpenHour()
		{
			setupPerson();
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(18, 1);
			Target.Process(personRequest, true);

			var timeZoneInfo = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(string.Format(Resources.OvertimeRequestDenyReasonOutOfSiteOpenHour,
					personRequest.Request.Period.StartDateTimeLocal(timeZoneInfo) + " - " +
					personRequest.Request.Period.EndDateTimeLocal(timeZoneInfo)
					, "8:00:00 AM-5:00:00 PM"));
		}

		[Test]
		public void ShouldDenyWhenSiteOpenHourIsClosed()
		{
			setupPerson(8, 17, true);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(8, 1);
			Target.Process(personRequest, true);

			var timeZoneInfo = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(string.Format(Resources.OvertimeRequestDenyReasonSiteOpenHourClosed,
					personRequest.Request.Period.StartDateTimeLocal(timeZoneInfo) + " - " +
					personRequest.Request.Period.EndDateTimeLocal(timeZoneInfo)));
		}

		[Test]
		public void ShouldDenyForEditWhenThereIsNoUnderStaffingSkill()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 15d);

			var personRequest = createOvertimeRequest(18, 1);
			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.NoUnderStaffingSkill);
		}

		[Test]
		public void ShouldDenyWhenThereIsNoUnderStaffingSkill()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 15d);

			var personRequest = createOvertimeRequest(18, 1);
			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.NoUnderStaffingSkill);
		}

		[Test]
		public void ShouldDenyWhenAnySkillIsNotCriticalUnderStaffing()
		{
			setupPerson();

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");

			var notUnderStaffingSkill = createSkill("notUnderStaffingSkill");
			var criticalUnderStaffingSkill = createSkill("criticalUnderStaffingSkill");

			var personSkill1 = createPersonSkill(activity1, notUnderStaffingSkill);
			var personSkill2 = createPersonSkill(activity2, criticalUnderStaffingSkill);

			setupIntradayStaffingForSkill(notUnderStaffingSkill, 10d, 15d);
			setupIntradayStaffingForSkill(criticalUnderStaffingSkill, 10d, 6d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			Target.CheckAndProcessDeny(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.NoUnderStaffingSkill);
		}

		[Test]
		public void ShouldDenyWhenOnlyUnderStaffingButNoCriticalSkill()
		{
			setupPerson();
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 15d);

			var personRequest = createOvertimeRequest(11, 1);
			mockRequestApprovalServiceApproved(personRequest);

			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.NoUnderStaffingSkill);
		}

		[Test]
		public void ShouldDenyWhenSkillOpenHourIsNotAvailable()
		{
			setupPerson(8, 23);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 6d);

			var personRequest = createOvertimeRequest(21, 1);
			mockRequestApprovalServiceApproved(personRequest);

			Target.CheckAndProcessDeny(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.PeriodIsOutOfSkillOpenHours);
		}

		[Test]
		public void ShouldDenyOvertimeRequestWhenUserHasNoSkill()
		{
			setupPerson(8, 21);

			var personRequest = createOvertimeRequest(18, 1);
			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.NoAvailableSkillForOvertime);
		}

		[Test]
		public void ShouldDenyForEditOvertimeRequestWhenOutOfSkillOpenHour()
		{
			setupPerson(8, 23);
			setupPersonSkill();

			var personRequest = createOvertimeRequest(22, 1);
			Target.CheckAndProcessDeny(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.PeriodIsOutOfSkillOpenHours);
		}

		[Test]
		public void ShouldDenyOvertimeRequestWhenOutOfSkillOpenHour()
		{
			setupPerson(8, 23);
			setupPersonSkill();

			var personRequest = createOvertimeRequest(22, 1);
			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.PeriodIsOutOfSkillOpenHours);
		}

		[Test]
		public void ShouldDenyRequestWhenApproveFailed()
		{
			setupPerson();

			var requestStartTime = Now.UtcDateTime().AddMinutes(MinimumApprovalThresholdTimeInMinutes + 1);
			var overtimeRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));

			var requestApprovalService = MockRepository.GenerateMock<IRequestApprovalService>();
			requestApprovalService.Stub(r => r.Approve(overtimeRequest.Request)).Return(new IBusinessRuleResponse[] { new BusinessRuleResponse(null, "error", true, false, overtimeRequest.Request.Period, overtimeRequest.Person, DateOnly.Today.ToDateOnlyPeriod(), string.Empty) });
			RequestApprovalServiceFactory.SetApprovalService(requestApprovalService);

			Target.Process(overtimeRequest, true);

			Assert.AreEqual(overtimeRequest.IsDenied, true);
		}

		[Test]
		public void ShouldDenyForEditWhenThereIsScheduleWithinRequestPeriod()
		{
			var timeZoneInfo = TimeZoneInfo.Utc;
			var person = createPersonWithSiteOpenHours(8, 20);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(timeZoneInfo);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(17, 1);

			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 18);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			Target.CheckAndProcessDeny(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(string.Format(Resources.OvertimeRequestAlreadyHasScheduleInPeriod,
					personRequest.Request.Period.StartDateTimeLocal(timeZoneInfo),
					personRequest.Request.Period.EndDateTimeLocal(timeZoneInfo)));
		}

		[Test]
		public void ShouldDenyWhenThereIsScheduleWithinRequestPeriod()
		{
			var timeZoneInfo = TimeZoneInfo.Utc;
			var person = createPersonWithSiteOpenHours(8, 20);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(timeZoneInfo);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(17, 1);

			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 18);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			Target.Process(personRequest, true);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(string.Format(Resources.OvertimeRequestAlreadyHasScheduleInPeriod,
					personRequest.Request.Period.StartDateTimeLocal(timeZoneInfo),
					personRequest.Request.Period.EndDateTimeLocal(timeZoneInfo)));
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldDenyWhenThereIsNoOvertimeRequestOpenPeriod()
		{
			setupPerson(8, 21);
			LoggedOnUser.CurrentUser().WorkflowControlSet = new WorkflowControlSet();
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(18, 1);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(Resources.OvertimeRequestDenyReasonClosedPeriod);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldPendingWhenAutoGrantOfOpenPeriodIsNo()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsPending.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldApproveWhenRequestPeriodIsWithinAnOpenPeriod()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldApproveWhenRequestPeriodIsWithinAllOpenPeriods()
		{
			setupPerson(8, 21);
			var now = new DateOnly(Now.UtcDateTime());
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(now, now.AddDays(13))
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(2, 13)
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 22, 8, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldDenyWhenRequestPeriodIsOutsideAnOpenPeriod()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 26, 8, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/25/2017.");
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldDenyWhenRequestPeriodIsOutsideAllOpenPeriods()
		{
			setupPerson(8, 21);
			var now = new DateOnly(Now.UtcDateTime());
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(now, now.AddDays(13))
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(now.AddDays(15), now.AddDays(20))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 26, 9, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldDenyWhenRequestPeriodIsWithinOpenPeriodWithLargerOrderIndexAndAutoGrantIsDeny()
		{
			setupPerson(8, 21);
			var now = new DateOnly(Now.UtcDateTime());
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(now, now.AddDays(8)),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(now.AddDays(5), now.AddDays(9)),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(now.AddDays(6), now.AddDays(10)),
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 19, 9, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(Resources.OvertimeRequestDenyReasonAutodeny);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldApproveWhenRequestPeriodIsWithinOpenPeriodWithLargerOrderIndexAndAutoGrantIsYes()
		{
			setupPerson(8, 21);
			var now = new DateOnly(Now.UtcDateTime());
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(now, now.AddDays(8)),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(now.AddDays(6), now.AddDays(10)),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(now.AddDays(5), now.AddDays(9)),
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 19, 9, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldDenyWhenRequestPeriodIsWithinRollingOpenPeriodWithLargerOrderIndexAndAutoGrantIsDeny()
		{
			setupPerson(8, 21);
			var now = new DateOnly(Now.UtcDateTime());
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(now, now.AddDays(8)),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(now.AddDays(5), now.AddDays(9)),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(6, 10),
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 19, 9, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(Resources.OvertimeRequestDenyReasonAutodeny);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldApproveWhenRequestPeriodIsWithinRollingOpenPeriodWithLargerOrderIndexAndAutoGrantIsYes()
		{
			setupPerson(8, 21);
			var now = new DateOnly(Now.UtcDateTime());
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(now, now.AddDays(8)),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(6, 10),
				OrderIndex = 2
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(5, 9),
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 19, 9, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldDenyWhenRequestPeriodEndDateIsNotInOpenPeriod()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(1)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero,TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 21, 0, 0, DateTimeKind.Utc),6);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/13/2017.");
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldSuggestMultiplePeriodsExcludeDenyPeriods()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(4)))
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(1), new DateOnly(Now.UtcDateTime().AddDays(1)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 17, 21, 0, 0, DateTimeKind.Utc), 6);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/12/2017,7/14/2017 - 7/16/2017.");
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldSuggestMultiplePeriods()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(2)))
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(4), new DateOnly(Now.UtcDateTime().AddDays(6)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 15, 21, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/14/2017,7/16/2017 - 7/18/2017.");
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldSuggestMultiplePeriodsNoEarlierThanToday()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(-2), new DateOnly(Now.UtcDateTime().AddDays(2)))
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(4), new DateOnly(Now.UtcDateTime().AddDays(6)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 15, 21, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/14/2017,7/16/2017 - 7/18/2017.");
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldNotSuggestPeriodsWhenExpired()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(-4), new DateOnly(Now.UtcDateTime().AddDays(-1)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 15, 21, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request cannot be granted. Some dates are not open for requests at this time.");
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldSuggestMergedPeriods()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(5)))
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(2)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 18, 21, 0, 0, DateTimeKind.Utc), 1);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/17/2017.");
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldDenyWhenVoilateMaxWeekWorkTimeRule()
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
				WorkRuleValidationHandleType = OvertimeWorkRuleValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var pa = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 13, 8, 2017, 7, 13, 16));
			ScheduleStorage.Add(pa);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 2);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("The week contains too much work time (10:00). Max is 09:00.");
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldNotDenyWhenVoilateMaxWeekWorkTimeRuleWithToggle46417Off()
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
				WorkRuleValidationHandleType = OvertimeWorkRuleValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var pa = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, 13, 8, 2017, 7, 13, 16));
			ScheduleStorage.Add(pa);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 2);
			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldNotDenyWhenVoilateMaxWeekWorkTimeRuleAndHandleTypeIsPending()
		{
			setupPerson(8, 21);
			var person = LoggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(_periodStartDate.ToDateOnlyPeriod()).FirstOrDefault();
			personPeriod.PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(40),
				TimeSpan.FromHours(41), TimeSpan.FromHours(10), TimeSpan.FromHours(10));

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				EnableWorkRuleValidation = true,
				WorkRuleValidationHandleType = OvertimeWorkRuleValidationHandleType.Pending,
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

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 2);
			Target.Process(personRequest);

			personRequest.IsPending.Should().Be.True();
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
				WorkRuleValidationHandleType = OvertimeWorkRuleValidationHandleType.Deny,
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
			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldDenyWhenVoilateNightlyRestTimeRule()
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
				WorkRuleValidationHandleType = OvertimeWorkRuleValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero,TimeSpan.FromDays(1))), 10d, 8d);

			for (int i = 0; i < 5; i++)
			{
				var day = 10 + i;
				var pa = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, day, 8, 2017, 7, day, 16));
				ScheduleStorage.Add(pa);
			}

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 11);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("There must be a daily rest of at least 6:00 hours between 2 shifts. Between 7/13/2017 and 7/14/2017 there are only 5:00 hours.");
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
				WorkRuleValidationHandleType = OvertimeWorkRuleValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			for (int i = 0; i < 5; i++)
			{
				var day = 10 + i;
				var pa = createMainPersonAssignment(person, new DateTimePeriod(2017, 7, day, 8, 2017, 7, day, 16));
				ScheduleStorage.Add(pa);
			}

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 16, 0, 0, DateTimeKind.Utc), 10);
			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodWorkRuleSetting_46638)]
		public void ShouldDenyWhenVoilateWeeklyRestTimeRule()
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
				WorkRuleValidationHandleType = OvertimeWorkRuleValidationHandleType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(40)))
			});
			person.WorkflowControlSet = workflowControlSet;

			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 17, 4, 0, 0, DateTimeKind.Utc), 168);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("The week does not have the stipulated (30:00) weekly rest.");
		}

		private void mockRequestApprovalServiceApproved(IPersonRequest personRequest)
		{
			var requestApprovalService = MockRepository.GenerateMock<IRequestApprovalService>();
			requestApprovalService.Stub(r => r.Approve(personRequest.Request)).Return(new IBusinessRuleResponse[] { });
			RequestApprovalServiceFactory.SetApprovalService(requestApprovalService);
		}

		private IPersonAssignment createMainPersonAssignment(IPerson person, DateTimePeriod period)
		{
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			var main = ActivityFactory.CreateActivity("phone").WithId();
			main.AllowOverwrite = true;
			main.InWorkTime = true;
			return PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, CurrentScenario.Current(), main, period, shiftCategory).WithId();
		}

		private IPersonAssignment createAssignmentWithDayOff(IPerson person, DateOnly date)
		{
			return PersonAssignmentFactory.CreateAssignmentWithDayOff(person, CurrentScenario.Current(), date, TimeSpan.FromHours(24), TimeSpan.FromHours(0), TimeSpan.FromHours(12));
		}

		private IPersonRequest createOvertimeRequest(DateTimePeriod period)
		{
			var personRequestFactory = new PersonRequestFactory();

			var personRequest = personRequestFactory.CreatePersonRequest(LoggedOnUser.CurrentUser());
			var overTimeRequest = new OvertimeRequest(new MultiplicatorDefinitionSet("name", MultiplicatorType.Overtime),
				period);
			personRequest.Request = overTimeRequest;
			PersonRequestRepository.Add(personRequest);
			return personRequest;
		}

		private IPerson createPersonWithSiteOpenHours(int startHour, int endHour, bool isOpenHoursClosed = false)
		{
			var team = TeamFactory.CreateTeam("team", "site");
			var siteOpenHour = new SiteOpenHour
			{
				Parent = team.Site,
				TimePeriod = new TimePeriod(startHour, 0, endHour, 0),
				WeekDay = DayOfWeek.Monday,
				IsClosed = isOpenHoursClosed
			};
			team.Site.AddOpenHour(siteOpenHour);
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(_periodStartDate, team);
			return person;
		}

		private IActivity createActivity(string name)
		{
			var activity = ActivityFactory.CreateActivity(name);
			activity.RequiresSkill = true;
			return activity;
		}

		private IPersonSkill createPersonSkill(IActivity activity, ISkill skill)
		{
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private void addPersonSkillsToPersonPeriod(params IPersonSkill[] personSkills)
		{
			var personPeriod = getOrAddPersonPeriod(Now.ServerDate_DontUse());
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
		}

		private PersonPeriod getOrAddPersonPeriod(DateOnly startDate)
		{
			var personPeriod = (PersonPeriod)LoggedOnUser.CurrentUser().PersonPeriods(startDate.ToDateOnlyPeriod()).FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(startDate, PersonContractFactory.CreatePersonContract(), team);
			LoggedOnUser.CurrentUser().AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		private static StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}
		private ISkill createSkill(string name, TimePeriod? skillOpenHourPeriod = null)
		{
			var skill = SkillFactory.CreateSkill(name).WithId();
			skill.SkillType.Description = new Description("SkillTypeInboundTelephony");
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, skillOpenHourPeriod ?? _defaultOpenPeriod);
			SkillRepository.Has(skill);
			return skill;
		}

		private ISkill setupPersonSkill(TimePeriod? skillOpenHourPeriod = null)
		{
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1", skillOpenHourPeriod);
			var personSkill1 = createPersonSkill(activity1, skill1);
			addPersonSkillsToPersonPeriod(personSkill1);
			return skill1;
		}
		
		private void setupIntradayStaffingForSkill(ISkill skill, double forecastedStaffing,
	double scheduledStaffing)
		{
			var period = getAvailablePeriod();
			period.DayCollection().ToList().ForEach(day =>
			{
				var utcDate = TimeZoneHelper.ConvertToUtc(day.Date,
					LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());

				for (var i = 0; i < _intervals.Length; i++)
				{
					CombinationRepository.AddSkillCombinationResource(new DateTime(),
						new[]
						{
							new SkillCombinationResource
							{
								StartDateTime = utcDate.Add(_intervals[i]),
								EndDateTime = utcDate.Add(_intervals[i]).AddMinutes(15),
								Resource = scheduledStaffing,
								SkillCombination = new[] {skill.Id.Value}
							}
						});
				}

				var timePeriodTuples = new List<Tuple<TimePeriod, double>>();
				for (var i = 0; i < _intervals.Length; i++)
				{
					timePeriodTuples.Add(new Tuple<TimePeriod, double>(
						new TimePeriod(_intervals[i], _intervals[i].Add(TimeSpan.FromMinutes(15))),
						forecastedStaffing));
				}
				var skillDay = skill.CreateSkillDayWithDemandOnInterval(Scenario.Current(), day, 0,
					timePeriodTuples.ToArray());
				skillDay.SkillDataPeriodCollection.ForEach(s => { s.Shrinkage = new Percent(0.5); });
				SkillDayRepository.Has(skillDay);
			});
		}

		private DateOnlyPeriod getAvailablePeriod()
		{
			var today = Now.ServerDate_DontUse();
			var period = new DateOnlyPeriod(today, today.AddDays(13)).Inflate(1);
			return period;
		}

		private TimeSpan[] createIntervals()
		{
			var intervals = new List<TimeSpan>();
			for (var i = 00; i < 1440; i += 15)
			{
				intervals.Add(TimeSpan.FromMinutes(i));
			}
			return intervals.ToArray();
		}

		private void setupPerson(int siteOpenStartHour = 8, int siteOpenEndHour = 17, bool isOpenHoursClosed = false)
		{
			//var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var person = createPersonWithSiteOpenHours(siteOpenStartHour, siteOpenEndHour, isOpenHoursClosed);
			person.PermissionInformation.SetUICulture(CultureInfoFactory.CreateUsCulture());
			person.PermissionInformation.SetCulture(CultureInfoFactory.CreateUsCulture());
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfo.Utc);
			CurrentScenario.FakeScenario(new Scenario("default") { DefaultScenario = true });

		}

		private IPersonRequest createOvertimeRequest(int startHour, int duration)
		{
			var requestStartTime = new DateTime(2017, 7, 17, startHour, 0, 0, DateTimeKind.Utc);
			return createOvertimeRequest(requestStartTime, duration);
		}

		private IPersonRequest createOvertimeRequest(DateTime requestStartTime, int duration)
		{
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddHours(duration)));
			mockRequestApprovalServiceApproved(personRequest);
			return personRequest;
		}
	}
}
