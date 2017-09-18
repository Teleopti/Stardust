using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
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
	public class OvertimeRequestProcessorTest : ISetup
	{
		public IOvertimeRequestProcessor Target;
		public IPersonRequestRepository PersonRequestRepository;
		public FakeCurrentScenario CurrentScenario;
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

		private readonly TimePeriod _defaultOpenPeriod = new TimePeriod(8, 00, 21, 00);
		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);
		private TimeSpan[] _intervals;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<OvertimeRequestProcessor>().For<IOvertimeRequestProcessor>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<DoNothingScheduleDayChangeCallBack>().For<IScheduleDayChangeCallback>();
			system.UseTestDouble<SiteOpenHoursSpecification>().For<ISiteOpenHoursSpecification>();
			system.UseTestDouble<FakeRequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();
			system.UseTestDouble<ScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble(new ThisIsNow(new DateTime(2017, 07, 12, 10, 00, 00, DateTimeKind.Utc))).For<INow>();
			_intervals = createIntervals();
		}

		[Test]
		public void ShouldApprove()
		{
			setupPerson(8,21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(18, 1);
			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldNotLoadStaffingDataAgainWhenValidatedSkillsAreaProvided()
		{
			setupPerson(8, 21);
			var skill = setupPersonSkill();
			setupIntradayStaffingForSkill(skill, 10d, 5d);

			var personRequest = createOvertimeRequest(18, 1);
			RequestApprovalServiceFactory.SetApprovalService(new OvertimeRequestApprovalService(null, null, new FakeCommandDispatcher(), new [] { skill }));

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
			Target.Process(personRequest, true);

			var timeZoneInfo = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(string.Format(Resources.OvertimeRequestDenyReasonExpired, TimeZoneHelper.ConvertFromUtc(requestStartTime, timeZoneInfo), MinimumApprovalThresholdTimeInMinutes));
		}

		[Test]
		[SetCulture("en-US")]
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
		[SetCulture("en-US")]
		public void ShouldDenyWhenSiteOpenHourIsClosed()
		{
			setupPerson(8,17,true);
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
		[SetCulture("en-US")]
		public void ShouldDenyWhenThereIsNoUnderStaffingSkill()
		{
			setupPerson(8,21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 11d);

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

			setupIntradayStaffingForSkill(notUnderStaffingSkill, 10d, 10d);
			setupIntradayStaffingForSkill(criticalUnderStaffingSkill, 10d, 6d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.NoUnderStaffingSkill);
		}

		[Test]
		public void ShouldDenyWhenOnlyUnderStaffingButNoCriticalSkill()
		{
			setupPerson();
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 9d);

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

			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.PeriodIsOutOfSkillOpenHours);
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldDenyOvertimeRequestWhenUserHasNoSkill()
		{
			setupPerson(8,21);

			var personRequest = createOvertimeRequest(18, 1);
			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.NoAvailableSkillForOvertime);
		}

		[Test]
		[SetCulture("en-US")]
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
			return PersonAssignmentFactory.CreateAssignmentWithMainShift(person, CurrentScenario.Current(), main, period, shiftCategory);
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
		private ISkill createSkill(string name)
		{
			var skill = SkillFactory.CreateSkill(name).WithId();
			skill.SkillType.Description = new Description("SkillTypeInboundTelephony");
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, _defaultOpenPeriod);
			SkillRepository.Has(skill);
			return skill;
		}

		private ISkill setupPersonSkill()
		{
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1");
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
				SkillDayRepository.Has(skill.CreateSkillDayWithDemandOnInterval(Scenario.Current(), day, 0,
					timePeriodTuples.ToArray()));
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
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfo.Utc);
			CurrentScenario.FakeScenario(new Scenario("default") { DefaultScenario = true });

		}

		private IPersonRequest createOvertimeRequest(int startHour, int duration)
		{
			var requestStartTime = new DateTime(2017, 7, 17, startHour, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddHours(duration)));
			mockRequestApprovalServiceApproved(personRequest);
			return personRequest;
		}
	}
}
