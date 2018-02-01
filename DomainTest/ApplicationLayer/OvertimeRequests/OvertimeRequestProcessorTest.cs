using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
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
		public UpdatedBy UpdatedBy;
		public FakePersonRepository PersonRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public const int MinimumApprovalThresholdTimeInMinutes = 15;
		public MutableNow Now;
		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public IScheduleStorage ScheduleStorage;
		public FakeSkillRepository SkillRepository;
		public FakeSkillCombinationResourceRepository CombinationRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillTypeRepository SkillTypeRepository;
		public ICurrentScenario Scenario;
		public ISchedulingResultStateHolder SchedulingResultStateHolder;
		public FakeActivityRepository ActivityRepository;

		private readonly TimePeriod _defaultOpenPeriod = new TimePeriod(8, 00, 21, 00);
		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);
		private TimeSpan[] _intervals;

		private readonly IMultiplicatorDefinitionSet _multiplicatorDefinitionSet
			= new MultiplicatorDefinitionSet("name", MultiplicatorType.Overtime).WithId();

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<OvertimeRequestProcessor>().For<IOvertimeRequestProcessor>();
			system.UseTestDouble<UpdatedBy>().For<IUpdatedByScope>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<DoNothingScheduleDayChangeCallBack>().For<IScheduleDayChangeCallback>();
			system.UseTestDouble<SiteOpenHoursSpecification>().For<ISiteOpenHoursSpecification>();
			system.UseTestDouble(new FakeScenarioRepository(new Scenario("default") { DefaultScenario = true }))
				.For<IScenarioRepository>();

			var fakeMultiplicatorDefinitionSetRepository = new FakeMultiplicatorDefinitionSetRepository();
			fakeMultiplicatorDefinitionSetRepository.Has(_multiplicatorDefinitionSet);
			system.UseTestDouble(fakeMultiplicatorDefinitionSetRepository).For<IMultiplicatorDefinitionSetRepository>();
			system.UseTestDouble(fakeMultiplicatorDefinitionSetRepository).For<IProxyForId<IMultiplicatorDefinitionSet>>();

			system.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>>();
			system.UseTestDouble<ScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble(new MutableNow(new DateTime(2017, 07, 12, 10, 00, 00, DateTimeKind.Utc))).For<INow>();
			_intervals = createIntervals();
		}

		[Test]
		public void ShouldApproveUseIntradyShrinkage()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var personRequest = createOvertimeRequest(18, 1);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenThereIsNoUnderStaffingSkillWithShrinkage()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 15d);

			var personRequest = createOvertimeRequest(18, 1);
			getTarget().Process(personRequest, true);

			personRequest.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldNotLoadStaffingDataAgainWhenValidatedSkillsAreaProvided()
		{
			setupPerson(8, 21);
			var skill = setupPersonSkill();
			setupIntradayStaffingForSkill(skill, 10d, 5d);

			var personRequest = createOvertimeRequest(18, 1);

			getTarget().Process(personRequest, true);

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
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldNotApproveWhenAutoGrantoIsOff()
		{
			setupPerson();
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest, false);

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
			getTarget().CheckAndProcessDeny(personRequest);
			getTarget().Process(personRequest, true);

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
			getTarget().Process(personRequest, true);

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
			getTarget().Process(personRequest, true);

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
			getTarget().Process(personRequest, true);

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
			getTarget().Process(personRequest, true);

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
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var notUnderStaffingSkill = createSkill("notUnderStaffingSkill", null,timeZone);
			var criticalUnderStaffingSkill = createSkill("criticalUnderStaffingSkill",null, timeZone);

			var personSkill1 = createPersonSkill(activity1, notUnderStaffingSkill);
			var personSkill2 = createPersonSkill(activity2, criticalUnderStaffingSkill);

			setupIntradayStaffingForSkill(notUnderStaffingSkill, 10d, 15d);
			setupIntradayStaffingForSkill(criticalUnderStaffingSkill, 10d, 6d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().CheckAndProcessDeny(personRequest);

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

			getTarget().Process(personRequest, true);

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

			getTarget().CheckAndProcessDeny(personRequest);

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
			getTarget().Process(personRequest, true);

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
			getTarget().CheckAndProcessDeny(personRequest);

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
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.PeriodIsOutOfSkillOpenHours);
		}

		[Test]
		public void ShouldApproveOvertimeRequestWhenWithinSkillOpenHour()
		{
			setupPerson(8, 23);
			var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");//GMT-3
			
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(8, 19), timeZoneInfo), 10d, 5d);

			var personRequest = createOvertimeRequest(21, 1);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyRequestWhenApproveFailed()
		{
			setupPerson();

			var requestStartTime = Now.UtcDateTime().AddMinutes(MinimumApprovalThresholdTimeInMinutes + 1);
			var overtimeRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));

			getTarget().Process(overtimeRequest, true);

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

			getTarget().CheckAndProcessDeny(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(string.Format(Resources.OvertimeRequestAlreadyHasScheduleInPeriod,
					personRequest.Request.Period.StartDateTimeLocal(timeZoneInfo),
					personRequest.Request.Period.EndDateTimeLocal(timeZoneInfo)));
		}

		[Test]
		public void ShouldDenyForEditWhenThereIsCrossDayScheduleWithinRequestPeriod()
		{
			Now.Is(new DateTime(2017, 12, 25, 08, 0, 0, DateTimeKind.Utc));
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var workflowControlSet = new WorkflowControlSet();
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;
			var period = new DateTimePeriod(2017, 12, 31, 8, 2017, 12, 31, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period).WithId();
			ScheduleStorage.Add(assignment);

			var corssMonthPersonRequest = createOvertimeRequest(new DateTime(2017, 12, 31, 23, 0, 0, DateTimeKind.Utc), 2);
			getTarget().Process(corssMonthPersonRequest, true);

			corssMonthPersonRequest.IsApproved.Should().Be.True();

			var personRequest = createOvertimeRequest(new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc), 1);

			getTarget().CheckAndProcessDeny(personRequest);
			var timeZoneInfo = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

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

			getTarget().Process(personRequest, true);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

			personRequest.IsPending.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldApproveAndChangeUpdatedByToSystemUserWhenAutoGrantOfOpenPeriodIsYes()
		{
			PersonRepository.Add(PersonFactory.CreatePerson().WithId(SystemUser.Id));

			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
			UpdatedBy.Person().Id.Value.Should().Be(SystemUser.Id);
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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		public void ShouldDenyWhenStaffingDataOfRequestPeriodIsNotAvailable()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(27)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 26, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied automatically. The valid request period is 7/12/2017 - 7/25/2017.");
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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 21, 0, 0, DateTimeKind.Utc), 6);
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/17/2017.");
		}

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
				var startTime = TimeZoneHelper.ConvertToUtc(new DateTime(2017, 7, day, 14,0,0), timezone);
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

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024)]
		public void ShouldApproveWhenOvertimeRequestMaximumTimeIsDisabled()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumTimeEnabled = false
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024)]
		public void ShouldDenyWhenOvertimeRequestMaximumTimeIsZero()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumTimeEnabled = true,
					OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMaximumTime = TimeSpan.Zero
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "July", "03:00", "00:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024)]
		public void ShouldApproveWhenRequestTimeIsLessThanMaximumTime()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumTimeEnabled = true,
					OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMaximumTime = TimeSpan.FromHours(10)
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024)]
		public void ShouldApproveWhenRequestTimeEqualToMaximumTime()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumTimeEnabled = true,
					OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMaximumTime = TimeSpan.FromHours(3)
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024)]
		public void ShouldDenyWhenViolateOvertimeRequestMaximumTime()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumTimeEnabled = true,
					OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMaximumTime = TimeSpan.FromHours(2)
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "July", "03:00", "02:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024)]
		public void ShouldNotDenyWhenOvertimeRequestMaximumTimeHandleTypeIsSendToAdministrator()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumTimeEnabled = true,
					OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Pending,
					OvertimeRequestMaximumTime = TimeSpan.FromHours(2)
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;

			var personRequest = createOvertimeRequest(18, 3);
			personRequest.Pending();
			getTarget().Process(personRequest, true);

			personRequest.IsPending.Should().Be.True();
			personRequest.GetMessage(new NoFormatting()).Trim().Should()
				.Be(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "July", "03:00", "02:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024)]
		public void ShouldDenyWhenTotalOvertimeOfMonthExceedsMaximumTime()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumTimeEnabled = true,
					OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMaximumTime = TimeSpan.FromHours(10)
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;
			var period = new DateTimePeriod(2017, 7, 2, 8, 2017, 7, 2, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(),period).WithId();
			var main = ActivityFactory.CreateActivity("phone").WithId();
			main.AllowOverwrite = true;
			main.InWorkTime = true;
			assignment.AddOvertimeActivity(main,period,new MultiplicatorDefinitionSet("ot",MultiplicatorType.Overtime));
			ScheduleStorage.Add(assignment);

			var personRequest = createOvertimeRequest(18, 3);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "July", "13:00", "10:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024)]
		public void ShouldApproveWhenRequestTimeIsCrossMonth()
		{
			Now.Is(new DateTime(2017, 7, 30, 22, 0, 0, DateTimeKind.Utc));
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumTimeEnabled = true,
					OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMaximumTime = TimeSpan.FromHours(13)
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;
			var period = new DateTimePeriod(2017, 7, 2, 8, 2017, 7, 2, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period).WithId();
			var main = ActivityFactory.CreateActivity("phone").WithId();
			main.AllowOverwrite = true;
			main.InWorkTime = true;
			assignment.AddOvertimeActivity(main, period, new MultiplicatorDefinitionSet("ot", MultiplicatorType.Overtime));
			ScheduleStorage.Add(assignment);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 31, 22, 0, 0, DateTimeKind.Utc), 4);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024)]
		public void ShouldDenyWhenOvertimeOfSecondMonthExceedsMaximumTime()
		{
			Now.Is(new DateTime(2017, 7, 30, 22, 0, 0, DateTimeKind.Utc));
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumTimeEnabled = true,
					OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMaximumTime = TimeSpan.FromHours(13)
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;
			var period = new DateTimePeriod(2017, 7, 2, 8, 2017, 7, 2, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period).WithId();
			var main = ActivityFactory.CreateActivity("phone").WithId();
			main.AllowOverwrite = true;
			main.InWorkTime = true;
			assignment.AddOvertimeActivity(main, period, new MultiplicatorDefinitionSet("ot", MultiplicatorType.Overtime));
			ScheduleStorage.Add(assignment);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 31, 22, 0, 0, DateTimeKind.Utc), 16);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "August", "14:00", "13:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024)]
		public void ShouldDenyWhenOvertimeOfBothMonthsExceedsMaximumTime()
		{
			Now.Is(new DateTime(2017, 7, 30, 22, 0, 0, DateTimeKind.Utc));
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumTimeEnabled = true,
					OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMaximumTime = TimeSpan.FromHours(11)
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;
			var period = new DateTimePeriod(2017, 7, 2, 8, 2017, 7, 2, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period).WithId();
			var main = ActivityFactory.CreateActivity("phone").WithId();
			main.AllowOverwrite = true;
			main.InWorkTime = true;
			assignment.AddOvertimeActivity(main, period, new MultiplicatorDefinitionSet("ot", MultiplicatorType.Overtime));
			ScheduleStorage.Add(assignment);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 31, 22, 0, 0, DateTimeKind.Utc), 16);
			getTarget().Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should()
				.Contain(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "July", "12:00", "11:00"));
			personRequest.DenyReason.Trim().Should()
				.Contain(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "August", "14:00", "11:00"));
			personRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024)]
		public void ShouldAddCrossMonthOvertimeStartsFromLastMonth()
		{
			Now.Is(new DateTime(2017, 12, 25, 08, 0, 0, DateTimeKind.Utc));
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumTimeEnabled = true,
					OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMaximumTime = TimeSpan.FromHours(1)
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;
			var period = new DateTimePeriod(2017, 12, 31, 8, 2017, 12, 31, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period).WithId();
			ScheduleStorage.Add(assignment);

			var corssMonthPersonRequest = createOvertimeRequest(new DateTime(2017, 12, 31, 23, 0, 0, DateTimeKind.Utc), 2);
			getTarget().Process(corssMonthPersonRequest, true);

			corssMonthPersonRequest.IsApproved.Should().Be.True();
			
			var nextMonthPersonRequest = createOvertimeRequest(new DateTime(2018, 1, 1, 1, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(nextMonthPersonRequest, true);

			nextMonthPersonRequest.IsDenied.Should().Be.True();
			nextMonthPersonRequest.DenyReason.Should().Be(string.Format(Resources.OvertimeRequestMaximumTimeDenyReason, "January", "02:00", "01:00"));
			nextMonthPersonRequest.BrokenBusinessRules.Should().Be(BusinessRuleFlags.MaximumOvertimeRule);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestCheckCalendarMonthMaximumOvertime_47024)]
		public void ShouldSubtractCrossMonthOvertimeEndsInNextMonth()
		{
			Now.Is(new DateTime(2017, 12, 25, 08, 0, 0, DateTimeKind.Utc));
			setupPerson(0, 24);
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 8d);

			var workflowControlSet =
				new WorkflowControlSet
				{
					OvertimeRequestMaximumTimeEnabled = true,
					OvertimeRequestMaximumTimeHandleType = OvertimeValidationHandleType.Deny,
					OvertimeRequestMaximumTime = TimeSpan.FromHours(2)
				};
			var person = LoggedOnUser.CurrentUser();
			person.WorkflowControlSet = workflowControlSet;
			var period = new DateTimePeriod(2017, 12, 31, 8, 2017, 12, 31, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(person, Scenario.Current(), period).WithId();
			ScheduleStorage.Add(assignment);

			var corssMonthPersonRequest = createOvertimeRequest(new DateTime(2017, 12, 31, 23, 0, 0, DateTimeKind.Utc), 2);
			getTarget().Process(corssMonthPersonRequest, true);

			corssMonthPersonRequest.IsApproved.Should().Be.True();

			var nextMonthPersonRequest = createOvertimeRequest(new DateTime(2017, 12, 31, 22, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(nextMonthPersonRequest, true);

			nextMonthPersonRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldDenyWhenNoSkillTypeIsMatched()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				SkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId()
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should().Contain("Agent has no available skill for overtime");
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldApproveWhenSkillTypeIsMatched()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				SkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldApproveWhenSkillTypeIsNotSet()
		{
			setupPerson(8, 21);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});

			SkillTypeRepository.Add(new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony));

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
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

		private IPersonAssignment createMainPersonAssignment(IPerson person, DateTimePeriod period)
		{
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Day");
			var main = ActivityFactory.CreateActivity("phone").WithId();
			main.AllowOverwrite = true;
			main.InWorkTime = true;
			return PersonAssignmentFactory
				.CreateAssignmentWithMainShift(person, Scenario.Current(), main, period, shiftCategory).WithId();
		}

		private IPersonAssignment createMainPersonAssignmenDayoff(IPerson person, DateOnly day)
		{
			var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("Slackday", "SD"));
			return PersonAssignmentFactory.CreateAssignmentWithDayOff(person, Scenario.Current(), day, dayOffTemplate);
		}

		private IPersonRequest createOvertimeRequest(DateTimePeriod period)
		{
			var personRequestFactory = new PersonRequestFactory {Person = LoggedOnUser.CurrentUser()};

			var personRequest = personRequestFactory.CreateNewPersonRequest();
			var overTimeRequest = new OvertimeRequest(_multiplicatorDefinitionSet, period);
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
			activity.InWorkTime = true;
			ActivityRepository.Add(activity);
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

		private StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}

		private ISkill createSkill(string name, TimePeriod? skillOpenHourPeriod = null,TimeZoneInfo timeZone = null)
		{
			var skill = SkillFactory.CreateSkill(name,timeZone).WithId();
			skill.SkillType.Description = new Description("SkillTypeInboundTelephony");
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, skillOpenHourPeriod ?? _defaultOpenPeriod);
			SkillRepository.Has(skill);
			return skill;
		}

		private ISkill setupPersonSkill(TimePeriod? skillOpenHourPeriod = null, TimeZoneInfo timeZone = null)
		{
			if (timeZone == null) timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1", skillOpenHourPeriod, timeZone);
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
			var person = createPersonWithSiteOpenHours(siteOpenStartHour, siteOpenEndHour, isOpenHoursClosed);
			person.PermissionInformation.SetUICulture(CultureInfoFactory.CreateUsCulture());
			person.PermissionInformation.SetCulture(CultureInfoFactory.CreateUsCulture());
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfo.Utc);
		}

		private IPersonRequest createOvertimeRequest(int startHour, int duration)
		{
			var requestStartTime = new DateTime(2017, 7, 17, startHour, 0, 0, DateTimeKind.Utc);
			return createOvertimeRequest(requestStartTime, duration);
		}

		private IPersonRequest createOvertimeRequest(DateTime requestStartTime, int duration)
		{
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddHours(duration)));
			return personRequest;
		}

		private IOvertimeRequestProcessor getTarget()
		{
			Target.StaffingDataAvailableDays = 13;
			return Target;
		}
	}
}

