using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
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
	public partial class OvertimeRequestProcessorTest : IIsolateSystem, ITestInterceptor
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
		public FakeSkillRepository SkillRepository;
		public FakeSkillCombinationResourceRepository CombinationRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillTypeRepository SkillTypeRepository;
		public ICurrentScenario Scenario;
		public IScheduleStorage ScheduleStorage;
		public ISchedulingResultStateHolder SchedulingResultStateHolder;
		public FakeActivityRepository ActivityRepository;
		public FakeUserTimeZone UserTimeZone;
		public SkillIntradayStaffingFactory SkillIntradayStaffingFactory;

		private readonly TimePeriod _defaultOpenPeriod = new TimePeriod(8, 00, 21, 00);
		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);
		private TimeSpan[] _intervals;

		private readonly IMultiplicatorDefinitionSet _multiplicatorDefinitionSet
			= new MultiplicatorDefinitionSet("name", MultiplicatorType.Overtime).WithId();
		private readonly ISkillType _phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();
		private readonly ISkillType _emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
		private readonly ISkillType _chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId();

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<UpdatedBy>().For<IUpdatedByScope>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<DoNothingScheduleDayChangeCallBack>().For<IScheduleDayChangeCallback>();
			isolate.UseTestDouble<SiteOpenHoursSpecification>().For<ISiteOpenHoursSpecification>();
			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario("default") { DefaultScenario = true }))
				.For<IScenarioRepository>();

			var fakeMultiplicatorDefinitionSetRepository = new FakeMultiplicatorDefinitionSetRepository();
			fakeMultiplicatorDefinitionSetRepository.Has(_multiplicatorDefinitionSet);
			isolate.UseTestDouble(fakeMultiplicatorDefinitionSetRepository).For<IMultiplicatorDefinitionSetRepository>();
			isolate.UseTestDouble(fakeMultiplicatorDefinitionSetRepository).For<IProxyForId<IMultiplicatorDefinitionSet>>();

			isolate.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>>();
			isolate.UseTestDouble<MutableNow>().For<INow, IMutateNow>();
			isolate.UseTestDouble<SkillIntradayStaffingFactory>().For<SkillIntradayStaffingFactory>();
			_intervals = createIntervals();
		}

		public void OnBefore()
		{
			Now.Is(new DateTime(2017, 07, 12, 10, 00, 00, DateTimeKind.Utc));
		}
		
		[Test]
		public void ShouldApproveRequestInLessThan15Minutes()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var personRequest = createOvertimeRequestInMinutes(18, 10);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveRequestIn31Minutes()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var personRequest = createOvertimeRequestInMinutes(18, 31);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}


		[Test]
		public void ShouldDenyWhenThereIsNoUnderStaffingSkillWithShrinkage()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 15d);

			var personRequest = createOvertimeRequest(18, 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldNotLoadStaffingDataAgainWhenValidatedSkillsAreaProvided()
		{
			setupPerson(8, 21);
			var skill = setupPersonSkill();
			setupIntradayStaffingForSkill(skill, 10d, 5d);

			var personRequest = createOvertimeRequest(18, 1);

			getTarget().Process(personRequest);

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
			PersonAssignmentRepository.Add(pa);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldNotApproveWhenAutoGrantoIsOff()
		{
			setupPerson();
			LoggedOnUser.CurrentUser().WorkflowControlSet.OvertimeRequestOpenPeriods.First().AutoGrantType =
				OvertimeRequestAutoGrantType.No;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsPending.Should().Be.True();
		}

		[Test]
		public void ShouldNotDenyWhenAutoGrantoIsOff()
		{
			setupPerson();
			LoggedOnUser.CurrentUser().WorkflowControlSet.OvertimeRequestOpenPeriods.First().AutoGrantType =
				OvertimeRequestAutoGrantType.No;
			setupIntradayStaffingForSkill(setupPersonSkill(), 5d, 10d);

			var personRequest = createOvertimeRequest(8, 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsPending.Should().Be.True();
			personRequest.GetMessage(new NoFormatting()).Should().Be(Resources.NoUnderStaffingSkill);
		}

		[Test]
		public void ShouldDenyWhenItsStartTimeIsWithinUpcoming15Mins()
		{
			setupPerson();
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var requestStartTime = Now.UtcDateTime().AddMinutes(MinimumApprovalThresholdTimeInMinutes - 1);
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.NoUnderStaffingSkill);
		}

		[Test]
		public void ShouldDenyWhenThereIsNoUnderStaffingSkillForRequestIn16Minutes()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 15d);

			var personRequest = createOvertimeRequestInMinutes(18, 16);
			getTarget().Process(personRequest);

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
			var notUnderStaffingSkill2 = createSkill("notUnderStaffingSkill2", null, timeZone);

			var personSkill1 = createPersonSkill(activity1, notUnderStaffingSkill);
			var personSkill2 = createPersonSkill(activity2, notUnderStaffingSkill2);

			setupIntradayStaffingForSkill(notUnderStaffingSkill, 10d, 15d);
			setupIntradayStaffingForSkill(notUnderStaffingSkill2, 10d, 20d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);

			getTarget().Process(personRequest);

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

			getTarget().Process(personRequest);

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

			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.ThereIsNoAvailableSkillForOvertime);
		}

		[Test]
		public void ShouldDenyForEditOvertimeRequestWhenOutOfSkillOpenHour()
		{
			setupPerson(8, 23);
			setupPersonSkill();

			var personRequest = createOvertimeRequest(22, 1);

			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

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
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyRequestWhenApproveFailed()
		{
			setupPerson();

			var requestStartTime = Now.UtcDateTime().AddMinutes(MinimumApprovalThresholdTimeInMinutes + 1);
			var overtimeRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));

			getTarget().Process(overtimeRequest);

			Assert.AreEqual(overtimeRequest.IsDenied, true);
		}

		[Test]
		public void ShouldDenyForEditWhenThereIsScheduleWithinRequestPeriod()
		{
			var timeZoneInfo = TimeZoneInfo.Utc;
			var person = createPersonWithSiteOpenHours(8, 20);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(timeZoneInfo);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				BetweenDays = new MinMax<int>(0, 30)
			});
			person.WorkflowControlSet = workflowControlSet;
			SkillTypeRepository.Add(_phoneSkillType);

			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(17, 1);

			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 18);
			var pa = createMainPersonAssignment(person, period);
			PersonAssignmentRepository.Add(pa);

			getTarget().Process(personRequest);

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

			var period = new DateTimePeriod(2017, 12, 31, 8, 2017, 12, 31, 18);
			var assignment = PersonAssignmentFactory.CreateEmptyAssignment(LoggedOnUser.CurrentUser(), Scenario.Current(), period).WithId();
			PersonAssignmentRepository.Add(assignment);

			var corssMonthPersonRequest = createOvertimeRequest(new DateTime(2017, 12, 31, 23, 0, 0, DateTimeKind.Utc), 2);
			getTarget().Process(corssMonthPersonRequest);

			corssMonthPersonRequest.IsApproved.Should().Be.True();

			var personRequest = createOvertimeRequest(new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc), 1);
			var timeZoneInfo = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(string.Format(Resources.OvertimeRequestAlreadyHasScheduleInPeriod,
					personRequest.Request.Period.StartDateTimeLocal(timeZoneInfo),
					personRequest.Request.Period.EndDateTimeLocal(timeZoneInfo)));
		}

		[Test]
		public void ShouldDenyWhenThereIsScheduleWithinRequestPeriod()
		{
			setupPerson(8, 20);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(17, 1);

			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 18);
			var pa = createMainPersonAssignment(LoggedOnUser.CurrentUser(), period);
			PersonAssignmentRepository.Add(pa);

			getTarget().Process(personRequest);

			var timeZoneInfo = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(string.Format(Resources.OvertimeRequestAlreadyHasScheduleInPeriod,
					personRequest.Request.Period.StartDateTimeLocal(timeZoneInfo),
					personRequest.Request.Period.EndDateTimeLocal(timeZoneInfo)));
		}

		[Test]
		public void ShouldApproveWhenUsePrimarySkillValidationIsOnAndTheSkillIsCriticalUnderStaffed()
		{
			setupPerson(8, 20);
			LoggedOnUser.CurrentUser().WorkflowControlSet.OvertimeRequestUsePrimarySkill = true;

			var primarySkill = setupPersonSkill();
			primarySkill.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill, 10d, 1d);

			var nonPrimarySkill = setupPersonSkill();
			setupIntradayStaffingForSkill(nonPrimarySkill, 10d, 20d);

			var personRequest = createOvertimeRequest(18, 1);

			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 18);
			var pa = createMainPersonAssignment(LoggedOnUser.CurrentUser(), period);
			PersonAssignmentRepository.Add(pa);

			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenUsePrimarySkillValidationIsOffAndOnlyPrimarySkillIsNotCriticalUnderStaffed()
		{
			setupPerson(8, 20);
			LoggedOnUser.CurrentUser().WorkflowControlSet.OvertimeRequestUsePrimarySkill = false;

			var primarySkill = setupPersonSkill();
			primarySkill.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill, 10d, 20d);

			var nonPrimarySkill = setupPersonSkill();
			setupIntradayStaffingForSkill(nonPrimarySkill, 10d, 1d);

			var personRequest = createOvertimeRequest(18, 1);

			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 18);
			var pa = createMainPersonAssignment(LoggedOnUser.CurrentUser(), period);
			PersonAssignmentRepository.Add(pa);

			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
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

		private ISkill createSkill(string name, TimePeriod? skillOpenHourPeriod = null,TimeZoneInfo timeZone = null, ISkillType skillType = null)
		{
			var skill = SkillFactory.CreateSkill(name,timeZone).WithId();
			if (skillType != null)
			{
				skill.SkillType = skillType;
			}
			else
			{
				skill.SkillType = _phoneSkillType;
			}
			
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, skillOpenHourPeriod ?? _defaultOpenPeriod);
			SkillRepository.Has(skill);
			return skill;
		}

		private ISkill createSkillWithDifferentOpenHourPeriod(string name, IDictionary<DayOfWeek, TimePeriod> days, TimeZoneInfo timeZone = null, ISkillType skillType = null)
		{
			var skill = SkillFactory.CreateSkill(name, timeZone).WithId();
			if (skillType != null)
			{
				skill.SkillType = skillType;
			}
			else
			{
				skill.SkillType.Description = new Description(SkillTypeIdentifier.Phone);
				skill.SkillType.SetId(new Guid());
			}

			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHoursOnDays(skill, days);
			SkillRepository.Has(skill);
			return skill;
		}

		private ISkill setupPersonSkill(TimePeriod? skillOpenHourPeriod = null, TimeZoneInfo timeZone = null, ISkillType skillType = null)
		{
			if (timeZone == null) timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1", skillOpenHourPeriod, timeZone, skillType);
			var personSkill1 = createPersonSkill(activity1, skill1);
			addPersonSkillsToPersonPeriod(personSkill1);
			return skill1;
		}

		private void setupIntradayStaffingForSkill(ISkill skill, double forecastedStaffing, double scheduledStaffing)
		{
			var period = getAvailablePeriod();
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			period.DayCollection().ToList().ForEach(day =>
			{
				var utcDate = TimeZoneHelper.ConvertToUtc(day.Date, timeZone);
				var staffingPeriodList = new List<StaffingPeriodData>();
				for (var i = 0; i < _intervals.Length; i++)
				{
					staffingPeriodList.Add( new StaffingPeriodData
					{
						ForecastedStaffing = forecastedStaffing,
						ScheduledStaffing = scheduledStaffing,
						Period = new DateTimePeriod(utcDate.Add(_intervals[i]), utcDate.Add(_intervals[i]).AddMinutes(15))
					});
				}

				SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, new DateOnly(utcDate),
					staffingPeriodList, timeZone);

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
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 30)
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			SkillTypeRepository.Add(_phoneSkillType);
		}

		private IPersonRequest createOvertimeRequest(int startHour, int hours)
		{
			var requestStartTime = new DateTime(2017, 7, 17, startHour, 0, 0, DateTimeKind.Utc);
			return createOvertimeRequest(requestStartTime, hours);
		}

		private IPersonRequest createOvertimeRequestInMinutes(int startHour, int minutes, TimeZoneInfo timeZoneInfo = null)
		{
			timeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Utc;
			var requestStartTimeLocal = new DateTime(2017, 7, 17, startHour, 0, 0);
			var requestStartTime = TimeZoneHelper.ConvertToUtc(requestStartTimeLocal, timeZoneInfo);
			var personRequest =
				createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddMinutes(minutes)));
			return personRequest;
		}

		private IPersonRequest createOvertimeRequestInMinutes(TimeSpan startTime, int minutes, TimeZoneInfo timeZoneInfo = null)
		{
			timeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Utc;
			var requestStartTimeLocal = new DateTime(2017, 7, 17, 0, 0, 0).Add(startTime);
			var requestStartTime = TimeZoneHelper.ConvertToUtc(requestStartTimeLocal, timeZoneInfo);
			var personRequest =
				createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddMinutes(minutes)));
			return personRequest;
		}

		private IPersonRequest createOvertimeRequest(DateTime requestStartTime, int hours)
		{
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddHours(hours)));
			return personRequest;
		}

		private IOvertimeRequestProcessor getTarget(int staffingDataAvailableDays = 13)
		{
			Target.StaffingDataAvailableDays = staffingDataAvailableDays;
			return Target;
		}
	}
}

