﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
		public const int MinimumApprovalThresholdTimeInMinutes = 15;
		public INow Now;
		public FakeRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public IScheduleStorage ScheduleStorage;
		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<OvertimeRequestProcessor>().For<IOvertimeRequestProcessor>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<DoNothingScheduleDayChangeCallBack>().For<IScheduleDayChangeCallback>();
			system.UseTestDouble<OvertimeRequestStartTimeValidator>().For<IOvertimeRequestValidator>();
			system.UseTestDouble<OvertimeRequestSiteOpenHourValidator>().For<IOvertimeRequestValidator>();
			system.UseTestDouble<OvertimeRequestAlreadyHasScheduleValidator>().For<IOvertimeRequestValidator>();
			system.UseTestDouble<SiteOpenHoursSpecification>().For<ISiteOpenHoursSpecification>();
			system.UseTestDouble<FakeRequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();
			system.UseTestDouble<ScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble(new ThisIsNow(new DateTime(2017, 07, 12, 10, 00, 00, DateTimeKind.Utc))).For<INow>();
		}

		[Test]
		public void ShouldApproveOvertimeRequests()
		{
			var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var person = createPersonWithSiteOpenHours(8, 21);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(timeZoneInfo);
			CurrentScenario.FakeScenario(new Scenario("default") { DefaultScenario = true });

			var requestStartTime = new DateTime(2017, 7, 17, 0, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(person, new DateTimePeriod(requestStartTime.AddHours(18), requestStartTime.AddHours(19)));
			mockRequestApprovalServiceApproved(personRequest);

			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyOvertimeRequestWhenItsStartTimeIsWithinUpcoming15Mins()
		{
			var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var person = createPersonWithSiteOpenHours(8, 17);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(timeZoneInfo);
			CurrentScenario.FakeScenario(new Scenario("default") { DefaultScenario = true });

			var requestStartTime = Now.UtcDateTime().AddMinutes(MinimumApprovalThresholdTimeInMinutes - 1);

			var personRequest = createOvertimeRequest(person, new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));

			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(string.Format(Resources.OvertimeRequestDenyReasonExpired, TimeZoneHelper.ConvertFromUtc(requestStartTime, timeZoneInfo), MinimumApprovalThresholdTimeInMinutes));
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldDenyOvertimeRequestWhenOutofSiteOpenHour()
		{
			var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var person = createPersonWithSiteOpenHours(8, 17);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(timeZoneInfo);
			CurrentScenario.FakeScenario(new Scenario("default") { DefaultScenario = true });

			var requestStartTime = new DateTime(2017, 7, 17, 0, 0, 0, DateTimeKind.Utc);

			var personRequest = createOvertimeRequest(person,
				new DateTimePeriod(requestStartTime.AddHours(18), requestStartTime.AddHours(19)));

			Target.Process(personRequest);

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
			var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var person = createPersonWithSiteOpenHours(8, 17, true);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(timeZoneInfo);
			CurrentScenario.FakeScenario(new Scenario("default") { DefaultScenario = true });

			var requestStartTime = new DateTime(2017, 7, 17, 0, 0, 0, DateTimeKind.Utc);

			var personRequest = createOvertimeRequest(person,
				new DateTimePeriod(requestStartTime.AddHours(16), requestStartTime.AddHours(17)));

			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should()
				.Be.EqualTo(string.Format(Resources.OvertimeRequestDenyReasonSiteOpenHourClosed,
					personRequest.Request.Period.StartDateTimeLocal(timeZoneInfo) + " - " +
					personRequest.Request.Period.EndDateTimeLocal(timeZoneInfo)));
		}

		[Test]
		public void ShouldDenyRequestWhenApproveFailed()
		{
			var person = PersonFactory.CreatePerson();
			var requestStartTime = Now.UtcDateTime().AddMinutes(MinimumApprovalThresholdTimeInMinutes + 1);
			var overtimeRequest = createOvertimeRequest(person, new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));

			var requestApprovalService = MockRepository.GenerateMock<IRequestApprovalService>();
			requestApprovalService.Stub(r => r.Approve(overtimeRequest.Request)).Return(new IBusinessRuleResponse[] { new BusinessRuleResponse(null, "error", true, false, overtimeRequest.Request.Period, overtimeRequest.Person, DateOnly.Today.ToDateOnlyPeriod(), string.Empty) });
			RequestApprovalServiceFactory.SetApprovalService(requestApprovalService);

			Target.Process(overtimeRequest);

			Assert.AreEqual(overtimeRequest.IsDenied, true);
		}

		[Test]
		public void ShouldDenyWhenThereIsScheduleWithinRequestPeriod()
		{
			var timeZoneInfo = TimeZoneInfo.Utc;
			var person = createPersonWithSiteOpenHours(8, 20);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(timeZoneInfo);

			var requestStartTime = new DateTime(2017, 7, 17, 17, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(person, new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));

			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 18);
			var pa = createMainPersonAssignment(person, period);
			ScheduleStorage.Add(pa);

			Target.Process(personRequest);

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

			var requestStartTime = new DateTime(2017, 7, 17, 11, 0, 0, DateTimeKind.Utc);
			var personRequest = createOvertimeRequest(person, new DateTimePeriod(requestStartTime, requestStartTime.AddHours(1)));

			var period = new DateTimePeriod(2017, 7, 17, 8, 2017, 7, 17, 18);
			var pa = createMainPersonAssignment(person, period);
			var lunch = ActivityFactory.CreateActivity("lunch").WithId();
			lunch.InContractTime = false;
			pa.AddActivity(lunch, new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12));
			ScheduleStorage.Add(pa);

			mockRequestApprovalServiceApproved(personRequest);

			Target.Process(personRequest);

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

		private IPersonRequest createOvertimeRequest(IPerson person, DateTimePeriod period)
		{
			var personRequestFactory = new PersonRequestFactory();

			var personRequest = personRequestFactory.CreatePersonRequest(person);
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

	}
}
