using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
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
		public FakeOvertimeRequestUnderStaffingSkillProvider OvertimeRequestUnderStaffingSkillProvider;
		public const int MinimumApprovalThresholdTimeInMinutes = 15;
		public INow Now;
		public FakeRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public IScheduleStorage ScheduleStorage;
		private readonly TimePeriod _defaultOpenPeriod = new TimePeriod(8, 00, 21, 00);
		public FakeSkillRepository SkillRepository;
		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<OvertimeRequestProcessor>().For<IOvertimeRequestProcessor>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeOvertimeRequestUnderStaffingSkillProvider>().For<IOvertimeRequestUnderStaffingSkillProvider>();
			system.UseTestDouble<DoNothingScheduleDayChangeCallBack>().For<IScheduleDayChangeCallback>();
			system.UseTestDouble<SiteOpenHoursSpecification>().For<ISiteOpenHoursSpecification>();
			system.UseTestDouble<FakeRequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();
			system.UseTestDouble<ScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble(new ThisIsNow(new DateTime(2017, 07, 12, 10, 00, 00, DateTimeKind.Utc))).For<INow>();
		}

		[Test]
		public void ShouldApproveOvertimeRequests()
		{
			setupPerson(8,21);

			setupStaffingSkill();

			var requestStartTime = new DateTime(2017, 7, 17, 0, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime.AddHours(18), requestStartTime.AddHours(19)));
			mockRequestApprovalServiceApproved(personRequest);

			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
		}

		private void setupPerson(int siteOpenStartHour = 8, int siteOpenEndHour = 17,bool isOpenHoursClosed=false)
		{
			var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var person = createPersonWithSiteOpenHours(siteOpenStartHour, siteOpenEndHour,isOpenHoursClosed);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(timeZoneInfo);
			CurrentScenario.FakeScenario(new Scenario("default") { DefaultScenario = true });

		}

		[Test]
		public void ShouldNotApproveOvertimeRequestsWhenAutoGrantoIsff()
		{
			setupPerson();
			setupStaffingSkill();

			var requestStartTime = new DateTime(2017, 7, 17, 0, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime.AddHours(8), requestStartTime.AddHours(9)));
			mockRequestApprovalServiceApproved(personRequest);

			Target.Process(personRequest, false);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsPending.Should().Be.True();
		}

		[Test]
		public void ShouldDenyOvertimeRequestWhenItsStartTimeIsWithinUpcoming15Mins()
		{
			setupPerson();
			setupStaffingSkill();

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
		public void ShouldDenyOvertimeRequestWhenOutofSiteOpenHour()
		{
			setupPerson();
			setupStaffingSkill();

			var requestStartTime = new DateTime(2017, 7, 17, 0, 0, 0, DateTimeKind.Utc);

			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime.AddHours(18), requestStartTime.AddHours(19)));

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
		public void ShouldDenyOvertimeRequestWhenSiteOpenHourIsClosed()
		{
			setupPerson(8,17,true);
			setupStaffingSkill();

			var requestStartTime = new DateTime(2017, 7, 17, 0, 0, 0, DateTimeKind.Utc);

			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime.AddHours(8), requestStartTime.AddHours(9)));

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
		public void ShouldDenyOvertimeRequestWhenThereIsNoUnderStaffingSkill()
		{
			setupPerson(8,21);

			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			addPersonSkillsToPersonPeriod(personSkill1);

			var requestStartTime = new DateTime(2017, 7, 17, 0, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime.AddHours(18), requestStartTime.AddHours(19)));
			mockRequestApprovalServiceApproved(personRequest);

			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(Resources.NoUnderStaffingSkill);
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldDenyOvertimeRequestWhenUserHasNoSkill()
		{
			setupPerson(8,21);

			var requestStartTime = new DateTime(2017, 7, 17, 0, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime.AddHours(18), requestStartTime.AddHours(19)));
			mockRequestApprovalServiceApproved(personRequest);

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
			setupPerson(8, 21);
			setupStaffingSkill();

			var requestStartTime = new DateTime(2017, 7, 17, 0, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime.AddHours(22), requestStartTime.AddHours(23)));
			mockRequestApprovalServiceApproved(personRequest);

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
			setupStaffingSkill();

			var requestStartTime = new DateTime(2017, 7, 17, 17, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));

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
		public void ShouldApprovedWhenScheduleIsNotInContractTime()
		{
			var timeZoneInfo = TimeZoneInfo.Utc;
			var person = createPersonWithSiteOpenHours(8, 20);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(timeZoneInfo);
			setupStaffingSkill();

			var requestStartTime = new DateTime(2017, 7, 17, 11, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));

			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 18);
			var pa = createMainPersonAssignment(person, period);
			var lunch = ActivityFactory.CreateActivity("lunch").WithId();
			lunch.InContractTime = false;
			pa.AddActivity(lunch, new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12));
			ScheduleStorage.Add(pa);

			mockRequestApprovalServiceApproved(personRequest);

			Target.Process(personRequest, true);

			personRequest.IsApproved.Should().Be.True();
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

		private void setupStaffingSkill()
		{
			var activity1 = createActivity("activity1");
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			addPersonSkillsToPersonPeriod(personSkill1);
			OvertimeRequestUnderStaffingSkillProvider.AddSeriousUnderstaffingSkill(skill1);
		}
	}
}
