using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Ccc.WebTest.Core.Requests.DataProvider;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[DomainTest] 
	[WebTest] 
	[RequestsTest] 
	public class RequestsViewModelMappingTest : IIsolateSystem
	{
		public RequestsViewModelMapper Target;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeUserTimeZone UserTimeZone;
		public MutableNow Now;

		[Test]
		public void ShouldMapLink()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod())).WithId();

			var result = Target.Map(request);

			result.Link.rel.Should().Be("self");
			result.Link.href.Should().Be(request.Id.ToString());
			result.Link.Methods.Should().Contain("GET");
			result.Link.Methods.Should().Contain("DELETE");
			result.Link.Methods.Should().Contain("PUT");
		}

		[Test]
		public void ShouldMapLinkForOfferStatusPending()
		{
			var result = stubRequestLinkForShiftExchangeOffer(ShiftExchangeOfferStatus.Pending, false);

			result.Link.rel.Should().Be("self");
			result.Link.href.Should().Be(result.Id);
			result.Link.Methods.Should().Contain("GET");
			result.Link.Methods.Should().Contain("DELETE");
			result.Link.Methods.Should().Contain("PUT");
		}

		[Test]
		public void ShouldMapLinkForOfferStatusExpired()
		{
			var result = stubRequestLinkForShiftExchangeOffer(ShiftExchangeOfferStatus.Pending, true);

			assertLinkForShiftExchageOfferWhenReadOnly(result);
		}

		[Test]
		public void ShouldMapLinkForOfferStatusCompleted()
		{
			var result = stubRequestLinkForShiftExchangeOffer(ShiftExchangeOfferStatus.Completed, false);

			assertLinkForShiftExchageOfferWhenReadOnly(result);
		}

		[Test]
		public void ShouldMapLinkForOfferStatusInvalid()
		{
			var result = stubRequestLinkForShiftExchangeOffer(ShiftExchangeOfferStatus.Invalid, true);

			assertLinkForShiftExchageOfferWhenReadOnly(result);
		}

		private static void assertLinkForShiftExchageOfferWhenReadOnly(RequestViewModel result)
		{
			result.Link.rel.Should().Be("self");
			result.Link.href.Should().Be(result.Id);
			result.Link.Methods.Should().Contain("GET");
			result.Link.Methods.Should().Contain("DELETE");
			result.Link.Methods.Should().Not.Contain("PUT");
		}

		private RequestViewModel stubRequestLinkForShiftExchangeOffer(ShiftExchangeOfferStatus status, bool isExpired)
		{
			var offer = createShiftExchangeOffer(status, isExpired);
			var request = new PersonRequest(new Person(), offer).WithId();

			var result = Target.Map(request);
			return result;
		}

		[Test]
		public void ShouldNotMapLinksDeleteMethodIfStateApproved()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod())).WithId();
			request.Pending();
			request.Approve(MockRepository.GenerateMock<IRequestApprovalService>(),
				MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());

			var result = Target.Map(request);

			result.Link.Methods.Should().Not.Contain("DELETE");
		}

		[Test]
		public void ShouldMapPayload()
		{
			const string payLoadName = "this is the one";
			var abs = new Absence {Description = new Description(payLoadName)};
			var request = new PersonRequest(new Person(), new AbsenceRequest(abs, new DateTimePeriod(1900, 1, 1, 1900, 1, 2)));
			var result = Target.Map(request);
			result.Payload.Should().Be.EqualTo(payLoadName);
		}

		[Test]
		public void ShouldNotMapPayload()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			var result = Target.Map(request);
			result.Payload.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotMapLinksDeleteMethodIfStateDenied()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Deny(null, MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());
			var result = Target.Map(request);

			result.Link.Methods.Should().Not.Contain("DELETE");
		}

		[Test]
		public void ShouldNotMapLinksPutMethodIfStateApproved()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Approve(MockRepository.GenerateMock<IRequestApprovalService>(),
				MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());

			var result = Target.Map(request);

			result.Link.Methods.Should().Not.Contain("PUT");
		}

		[Test]
		public void ShouldNotMapLinksPutMethodIfStateDenied()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Deny(null, MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());
			var result = Target.Map(request);

			result.Link.Methods.Should().Not.Contain("PUT");
		}

		[Test]
		public void ShouldMapLinksAppropriateMethodsIfStateWaitlisted()
		{
			var viewModel = getViewModelForWaitlistedRequest();
			viewModel.Link.Methods.Should().Contain("DELETE");
			viewModel.Link.Methods.Should().Contain("GET");
			viewModel.Link.Methods.Should().Not.Contain("PUT");
		}

		[Test]
		public void ShouldMapCancelMethodIfToggleOn()
		{
			UserTimeZone.IsSweden();
			
			var dateOnlyPeriod = Now.ServerDate_DontUse().AddDays(1).ToDateOnlyPeriod();
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var person = new Person();
			var request = createApprovedAbsenceRequest(absence, dateOnlyPeriod, person);

			var result = Target.Map(request);

			result.Link.Methods.Should().Contain("CANCEL");
		}


		[Test]
		public void ShouldMapId()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod())) {Subject = "Test"};
			request.SetId(Guid.NewGuid());

			var result = Target.Map(request);

			result.Id.Should().Be(request.Id.ToString());
		}

		[Test]
		public void ShouldMapSubject()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod())) {Subject = "Test"};
			var result = Target.Map(request);

			result.Subject.Should().Be("Test");
		}

		[Test]
		public void ShouldMapDateWithoutSeconds()
		{
			UserTimeZone.IsSweden();
			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(5));
			var request = new PersonRequest(new Person(), new TextRequest(period));
			var result = Target.Map(request);

			result.DateTimeFrom.Should().Be
				.EqualTo(DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(
					period.StartDateTime.Truncate(TimeSpan.FromMinutes(1)),
					UserTimeZone.TimeZone()));
			result.DateTimeTo.Should().Be
				.EqualTo(DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(period.EndDateTime.Truncate(TimeSpan.FromMinutes(1)),
					UserTimeZone.TimeZone()));

			DateTime.Parse(result.DateTimeFrom).Kind.Should().Be(DateTimeKind.Unspecified);
			DateTime.Parse(result.DateTimeTo).Kind.Should().Be(DateTimeKind.Unspecified);
		}

		[Test]
		public void ShouldMapDatesForShiftRequestForOneDate()
		{
			UserTimeZone.IsSweden();
			LoggedOnUser.CurrentUser().PermissionInformation.SetDefaultTimeZone(UserTimeZone.TimeZone());

			var startDate = DateOnly.Today;
			var request = createShiftTrade(LoggedOnUser.CurrentUser(), PersonFactory.CreatePerson("Receiver"),
				startDate, startDate);

			var result = Target.Map(request);

			new DateOnly(DateTime.Parse(result.DateTimeFrom)).Should().Be.EqualTo(startDate);
			new DateOnly(DateTime.Parse(result.DateTimeTo)).Should().Be.EqualTo(startDate);

		}

		[Test]
		public void ShouldMapDatesForShiftRequestForDatePeriod()
		{
			UserTimeZone.IsSweden();
			LoggedOnUser.CurrentUser().PermissionInformation.SetDefaultTimeZone(UserTimeZone.TimeZone());

			var startDate = DateOnly.Today;
			var endDate = startDate.AddDays(5);

			var request = createShiftTrade(LoggedOnUser.CurrentUser(), PersonFactory.CreatePerson("Receiver"),
				startDate, endDate);

			var result = Target.Map(request);

			new DateOnly(DateTime.Parse(result.DateTimeFrom)).Should().Be.EqualTo(startDate);
			new DateOnly(DateTime.Parse(result.DateTimeTo)).Should().Be.EqualTo(endDate);
		}

		[Test]
		public void ShouldMapType()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			var result = Target.Map(request);

			result.Type.Should().Be(request.Request.RequestTypeDescription);
		}

		[Test]
		public void ShouldMapRequestTypeEnum()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			var result = Target.Map(request);

			result.TypeEnum.Should().Be(RequestType.TextRequest);
		}

		[Test]
		public void ShouldMapUpdatedOn()
		{
			UserTimeZone.IsSweden();
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod())) {UpdatedOn = DateTime.UtcNow};
			var result = Target.Map(request);

			result.UpdatedOnDateTime.Should()
				.Be(DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(request.UpdatedOn.Value, UserTimeZone.TimeZone()));

		}

		[Test]
		public void ShouldMapText()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.TrySetMessage("Message");

			var result = Target.Map(request);

			result.Text.Should().Be.EqualTo("Message");
		}

		[Test]
		public void ShouldMapStatus()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			var result = Target.Map(request);

			result.Status.Should().Be.EqualTo(request.StatusText);
		}

		[Test]
		public void ShouldMapRawTimeInfo()
		{
			UserTimeZone.IsSweden();
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 3, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var request = new PersonRequest(new Person(), new TextRequest(period));
			var startInCorrectTimezone = TimeZoneHelper.ConvertFromUtc(start, UserTimeZone.TimeZone());
			var endInCorrectTimezone = TimeZoneHelper.ConvertFromUtc(end, UserTimeZone.TimeZone());

			var result = Target.Map(request);

			result.DateTimeFrom.Should().Be.EqualTo(startInCorrectTimezone.ToString("o"));
			result.DateTimeTo.Should().Be.EqualTo(endInCorrectTimezone.ToString("o"));
		}

		[Test, SetCulture("ar-SA")]
		public void ShouldMapYearMonthAndDayParts()
		{
			var start = new DateTime(2013, 3, 18, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2013, 3, 19, 0, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var request = new PersonRequest(new Person(), new TextRequest(period));

			var result = Target.Map(request);

			var calendar = CultureInfo.CurrentCulture.Calendar;
			var dateFrom = TimeZoneInfo.ConvertTimeFromUtc(request.Request.Period.StartDateTime, UserTimeZone.TimeZone());
			var dateTo = TimeZoneInfo.ConvertTimeFromUtc(request.Request.Period.EndDateTime, UserTimeZone.TimeZone());

			result.DateFromYear.Should().Be.EqualTo(calendar.GetYear(dateFrom));
			result.DateFromMonth.Should().Be.EqualTo(calendar.GetMonth(dateFrom));
			result.DateFromDayOfMonth.Should().Be.EqualTo(calendar.GetDayOfMonth(dateFrom));

			result.DateToYear.Should().Be.EqualTo(calendar.GetYear(dateTo));
			result.DateToMonth.Should().Be.EqualTo(calendar.GetMonth(dateTo));
			result.DateToDayOfMonth.Should().Be.EqualTo(calendar.GetDayOfMonth(dateTo));
		}

		[Test]
		public void ShouldMapDenyReason()
		{
			var request = new PersonRequest(new Person(), new AbsenceRequest(new Absence(), new DateTimePeriod()));
			request.Deny("RequestDenyReasonNoWorkflow", new PersonRequestAuthorizationCheckerForTest());

			var result = Target.Map(request);

			result.DenyReason.Should().Be.EqualTo(Resources.RequestDenyReasonNoWorkflow);
		}

		[Test]
		public void ShouldMapAlreadyTranslatedDenyReason()
		{
			var denyReason = "You must work at 2011-09-23.";
			var request = new PersonRequest(new Person(), new AbsenceRequest(new Absence(), new DateTimePeriod()));
			request.Deny(denyReason, new PersonRequestAuthorizationCheckerForTest());

			var result = Target.Map(request);

			result.DenyReason.Should().Be.EqualTo(denyReason);
		}

		[Test]
		public void ShouldSetIsCreatedByUserToTrueIfTheSenderIsTheSamePersonAsTheLoggedOnUser()
		{
			var receiver = PersonFactory.CreatePerson("Receiver");
			var personRequest = createShiftTrade(LoggedOnUser.CurrentUser(), receiver);

			var result = Target.Map(personRequest);

			Assert.That(result.IsCreatedByUser, Is.True);
		}

		[Test]
		public void ShouldSetIsCreatedByUserToFalseIfTheSenderIsTheSamePersonAsTheLoggedOnUser()
		{
			var sender = PersonFactory.CreatePerson("Sender");
			var personRequest = createShiftTrade(sender, LoggedOnUser.CurrentUser());

			var result = Target.Map(personRequest);

			Assert.That(result.IsCreatedByUser, Is.False);
		}

		[Test]
		public void ShouldMapFrom()
		{
			var sender = PersonFactory.CreatePerson("Sender");
			var tradeDate = new DateOnly(2010, 1, 1);
			var loggedOnPerson = LoggedOnUser.CurrentUser();
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, loggedOnPerson, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {shiftTradeSwapDetail});
			var personRequest = new PersonRequest(loggedOnPerson) {Subject = "Subject of request", Request = shiftTradeRequest};

			var result = Target.Map(personRequest);

			Assert.That(result.From, Is.EqualTo(sender.Name.ToString(NameOrderOption.FirstNameLastName)));
		}

		[Test]
		public void ShouldMapTo()
		{
			var sender = PersonFactory.CreatePerson("Sender");
			var tradeDate = new DateOnly(2010, 1, 1);
			var loggedOnPerson = LoggedOnUser.CurrentUser();
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, loggedOnPerson, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {shiftTradeSwapDetail});
			var personRequest = new PersonRequest(loggedOnPerson) {Subject = "Subject of request", Request = shiftTradeRequest};

			var result = Target.Map(personRequest);

			Assert.That(result.To, Is.EqualTo(loggedOnPerson.Name.ToString(NameOrderOption.FirstNameLastName)));
		}

		[Test]
		public void ShouldMapFromAndToToEmptyStringIfNotShiftTradeRequest()
		{
			var personRequest = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));

			var result = Target.Map(personRequest);

			result.From.Should().Be.Empty();
			result.To.Should().Be.Empty();
		}

		[Test]
		public void ShouldMapShiftExchangeOfferStatusInPending()
		{
			var shiftExchangeOffer = createShiftExchangeOffer(ShiftExchangeOfferStatus.Pending, false);
			var requestViewModel = createRequestViewModelWithShiftExchangeOffer(shiftExchangeOffer);
			requestViewModel.Status.Should().Contain(Resources.Pending);
		}

		[Test]
		public void ShouldMapShiftExchangeOfferStatusCompleted()
		{
			var shiftExchangeOffer = createShiftExchangeOffer(ShiftExchangeOfferStatus.Completed, false);
			var requestViewModel = createRequestViewModelWithShiftExchangeOffer(shiftExchangeOffer);
			requestViewModel.Status.Should().Contain(Resources.Completed);
		}

		[Test]
		public void ShouldMapShiftExchangeOfferStatusInvalid()
		{
			var shiftExchangeOffer = createShiftExchangeOffer(ShiftExchangeOfferStatus.Invalid, false);
			var requestViewModel = createRequestViewModelWithShiftExchangeOffer(shiftExchangeOffer);
			requestViewModel.Status.Should().Contain(Resources.Invalid);
		}

		[Test]
		public void ShouldMapShiftExchangeOfferStatusExpired()
		{
			var shiftExchangeOffer = createShiftExchangeOffer(ShiftExchangeOfferStatus.Pending, true);

			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser(), shiftExchangeOffer);
			var result = Target.Map(personRequest);

			result.Status.Should().Contain(Resources.Expired);
		}

		[Test]
		public void ShouldMapShiftExchangeOfferNextDayTrue()
		{
			UserTimeZone.IsSweden();
			var period = new DateTimePeriod(2015, 1, 30, 8, 2015, 1, 31, 0);
			var shiftExchangeOffer = createShiftExchangeOffer(period);

			var requestViewModel = createRequestViewModelWithShiftExchangeOffer(shiftExchangeOffer);

			requestViewModel.IsNextDay.Should().Be.True();
		}

		[Test]
		public void ShouldMapShiftExchangeOfferNextDayFalse()
		{
			var period = new DateTimePeriod(2015, 1, 30, 8, 2015, 1, 30, 10);
			var shiftExchangeOffer = createShiftExchangeOffer(period);

			var requestViewModel = createRequestViewModelWithShiftExchangeOffer(shiftExchangeOffer);

			requestViewModel.IsNextDay.Should().Be.False();
		}

		[Test]
		public void ShouldMapExchangeOfferViewModelWhenRequestIsShiftTrade()
		{
			var personRequest =
				new PersonRequest(LoggedOnUser.CurrentUser(), new ShiftTradeRequest(new List<IShiftTradeSwapDetail>()));

			var result = Target.Map(personRequest);

			result.ExchangeOffer.IsOfferAvailable.Should().Be.True();
		}

		[Test]
		public void ShouldMapShiftTradeStatusOkByMeWhenUserHasCreatedTheShiftTradeRequest()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			str.Expect(c => c.GetShiftTradeStatus(null)).IgnoreArguments().Return(ShiftTradeStatus.OkByMe);
			str.Expect(c => c.PersonFrom).Return(LoggedOnUser.CurrentUser());
			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser(), str);
			personRequest.ForcePending();

			var result = Target.Map(personRequest);
			result.Status.Should().Contain(Resources.WaitingForOtherPart);
		}

		[Test]
		public void ShouldMapShiftTradeStatusOkByMeWhenUserHasRecievedTheShiftTradeRequest()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			str.Expect(c => c.GetShiftTradeStatus(null)).IgnoreArguments().Return(ShiftTradeStatus.OkByMe);
			var person = new Person();
			str.Expect(c => c.PersonFrom).Return(person);
			var personRequest = new PersonRequest(person, str);
			personRequest.ForcePending();

			var result = Target.Map(personRequest);
			result.Status.Should().Contain(Resources.WaitingForYourApproval);
		}

		[Test]
		public void ShouldMapShiftTradeStatusOkByBothParts()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			str.Expect(c => c.GetShiftTradeStatus(null)).IgnoreArguments().Return(ShiftTradeStatus.OkByBothParts);
			var personRequest = new PersonRequest(new Person(), str);
			personRequest.ForcePending();

			var result = Target.Map(personRequest);
			result.Status.Should().Contain(Resources.WaitingForSupervisorApproval);
		}

		[Test]
		public void ShouldOnlyMapShiftTradeStatusWhenPending()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			var personRequest = new PersonRequest(new Person(), str);

			var result = Target.Map(personRequest);
			result.Status.Should().Not.Contain(",");

			str.AssertWasNotCalled(x => x.GetShiftTradeStatus(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldMapShiftTradeStatusReferred()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			str.Expect(c => c.GetShiftTradeStatus(null)).IgnoreArguments().Return(ShiftTradeStatus.Referred);
			var personRequest = new PersonRequest(new Person(), str);
			personRequest.ForcePending();

			var result = Target.Map(personRequest);
			result.Status.Should().Contain(Resources.TheScheduleHasChanged);
		}

		[Test]
		public void ShouldThrowShiftTradeStatusNotValid()
		{
			//AFAIK - this status is not in used. Cannot be deleted due to SDK thingys (?).
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			str.Expect(c => c.GetShiftTradeStatus(null)).IgnoreArguments().Return(ShiftTradeStatus.NotValid);
			var personRequest = new PersonRequest(new Person(), str);
			personRequest.ForcePending();

			Assert.Throws<ArgumentException>(() =>
				Target.Map(personRequest));
		}

		[Test]
		public void ShouldMapIsNew()
		{
			var sender = PersonFactory.CreatePerson();
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, LoggedOnUser.CurrentUser(), tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {shiftTradeSwapDetail});
			var personRequest =
				new PersonRequest(LoggedOnUser.CurrentUser()) {Subject = "Subject of request", Request = shiftTradeRequest};

			var result = Target.Map(personRequest);

			Assert.That(personRequest.IsNew);
			Assert.That(result.IsNew, Is.True);

			shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.Referred, new PersonRequestAuthorizationCheckerForTest());
			result = Target.Map(personRequest);

			Assert.That(personRequest.IsNew, Is.False);
			Assert.That(result.IsNew, Is.False);

		}

		[Test]
		public void ShouldMapIsPending()
		{
			var sender = PersonFactory.CreatePerson();
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, LoggedOnUser.CurrentUser(), tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {shiftTradeSwapDetail});
			var personRequest =
				new PersonRequest(LoggedOnUser.CurrentUser()) {Subject = "Subject of request", Request = shiftTradeRequest};
			var result = Target.Map(personRequest);

			Assert.That(personRequest.IsNew);
			Assert.That(result.IsPending, Is.False);

			personRequest.ForcePending();
			result = Target.Map(personRequest);

			Assert.That(personRequest.IsPending, Is.True);
			Assert.That(result.IsPending, Is.True);
		}

		[Test]
		public void ShouldMapIsApproved()
		{
			var sender = PersonFactory.CreatePerson();
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, LoggedOnUser.CurrentUser(), tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {shiftTradeSwapDetail});
			var personRequest =
				new PersonRequest(LoggedOnUser.CurrentUser()) {Subject = "Subject of request", Request = shiftTradeRequest};
			var result = Target.Map(personRequest);

			Assert.That(personRequest.IsApproved, Is.False);
			Assert.That(result.IsApproved, Is.False);

			personRequest.ForcePending();
			personRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());
			result = Target.Map(personRequest);

			Assert.That(personRequest.IsApproved, Is.True);
			Assert.That(result.IsApproved, Is.True);
		}

		[Test]
		public void ShouldMapIsDenied()
		{
			var sender = PersonFactory.CreatePerson();
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, LoggedOnUser.CurrentUser(), tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {shiftTradeSwapDetail});
			var personRequest =
				new PersonRequest(LoggedOnUser.CurrentUser()) {Subject = "Subject of request", Request = shiftTradeRequest};
			var result = Target.Map(personRequest);

			Assert.That(personRequest.IsDenied, Is.False);
			Assert.That(result.IsApproved, Is.False);

			personRequest.ForcePending();
			personRequest.Deny("MyDenyReason", new PersonRequestAuthorizationCheckerForTest());
			result = Target.Map(personRequest);

			Assert.That(personRequest.IsDenied, Is.True);
			Assert.That(result.IsDenied, Is.True);
		}

		[Test]
		public void ShouldMapIsReferred()
		{
			var sender = PersonFactory.CreatePerson();
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, LoggedOnUser.CurrentUser(), tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {shiftTradeSwapDetail});
			var personRequest =
				new PersonRequest(LoggedOnUser.CurrentUser()) {Subject = "Subject of request", Request = shiftTradeRequest};
			var result = Target.Map(personRequest);

			Assert.That(result.IsReferred, Is.False);

			shiftTradeRequest.Refer(new PersonRequestAuthorizationCheckerForTest());
			result = Target.Map(personRequest);

			Assert.That(result.IsReferred, Is.True);
		}

		[Test]
		public void ShouldHandleEmptyShiftSwapDetail()
		{
			var shiftTradeSwapDetails = new List<IShiftTradeSwapDetail>();
			var shifTradeRequest = new ShiftTradeRequest(shiftTradeSwapDetails);
			var personRequest = new PersonRequest(PersonFactory.CreatePerson(), shifTradeRequest);
			var shiftTradeRequestModel = Target.Map(personRequest);

			Assert.That(shiftTradeRequestModel.From, Is.Null.Or.Empty);
			Assert.That(shiftTradeRequestModel.To, Is.Null.Or.Empty);
		}

		[Test]
		public void ShouldMapMultiplicatorDefinitionSet()
		{
			var definitionSet = MultiplicatorDefinitionSetFactory
				.CreateMultiplicatorDefinitionSet("test", MultiplicatorType.Overtime).WithId();
			var overtimeRequest = new OvertimeRequest(definitionSet, new DateTimePeriod());
			var personRequest = new PersonRequest(PersonFactory.CreatePerson(), overtimeRequest);

			var overtimeRequestModel = Target.Map(personRequest);

			Assert.AreEqual(overtimeRequestModel.MultiplicatorDefinitionSet, definitionSet.Id.ToString());
		}

		private static IPersonRequest createShiftTrade(IPerson from, IPerson to)
		{
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(from, to, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {shiftTradeSwapDetail});
			var personRequest = new PersonRequest(from) {Subject = "Subject of request", Request = shiftTradeRequest};
			personRequest.TrySetMessage("This is a short text for the description of a shift trade request");
			personRequest.SetId(Guid.Empty);
			return personRequest;
		}

		private static IPersonRequest createShiftTrade(IPerson from, IPerson to, DateOnly fromDate, DateOnly toDate)
		{
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(from, to, fromDate, toDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {shiftTradeSwapDetail});
			var personRequest = new PersonRequest(from) {Subject = "Subject of request", Request = shiftTradeRequest};
			personRequest.TrySetMessage("This is a short text for the description of a shift trade request");
			personRequest.SetId(Guid.Empty);
			return personRequest;
		}

		private RequestViewModel createRequestViewModelWithShiftExchangeOffer(ShiftExchangeOffer shiftExchangeOffer)
		{
			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser(), shiftExchangeOffer);
			var result = Target.Map(personRequest);
			return result;

		}

		private ShiftExchangeOffer createShiftExchangeOffer(ShiftExchangeOfferStatus status, bool isExpired)
		{
			var currentShift = ScheduleDayFactory.Create(isExpired ? Now.ServerDate_DontUse().AddDays(-1) : Now.ServerDate_DontUse().AddDays(1));
			var str = new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(), status);
			return str;
		}

		private ShiftExchangeOffer createShiftExchangeOffer(DateTimePeriod period)
		{
			var scheduleDayFilterCriteria = new ScheduleDayFilterCriteria(ShiftExchangeLookingForDay.WorkingShift, period);
			var periodStartDate = new DateOnly(period.StartDateTime);
			var shiftExchangeCriteria = new ShiftExchangeCriteria(periodStartDate.AddDays(-1), scheduleDayFilterCriteria);
			var currentShift = ScheduleDayFactory.Create(periodStartDate.AddDays(-2));
			return new ShiftExchangeOffer(currentShift, shiftExchangeCriteria, ShiftExchangeOfferStatus.Pending);
		}

		private RequestViewModel getViewModelForWaitlistedRequest()
		{
			var absence = new Absence();
			var person = PersonFactory.CreatePersonWithId();
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 2, 3, 2015, 3, 3));

			var request = new PersonRequest(person, absenceRequest);
			request.SetId(Guid.NewGuid());

			var workflowControlSet = MockRepository.GenerateMock<WorkflowControlSet>();
			workflowControlSet.Stub(x => x.WaitlistingIsEnabled(absenceRequest)).Return(true);
			person.WorkflowControlSet = workflowControlSet;

			request.Deny(null, MockRepository.GenerateMock<IPersonRequestCheckAuthorization>(), null,
				PersonRequestDenyOption.AutoDeny);

			return Target.Map(request);
		}
		
		private static PersonRequest createApprovedAbsenceRequest(IAbsence absence, DateOnlyPeriod dateOnlyPeriod,
			IPerson person)
		{
			var absenceRequest = new AbsenceRequest(absence,
				dateOnlyPeriod.ToDateTimePeriod(TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			var request = new PersonRequest(person, absenceRequest).WithId();
			request.Pending();

			var approvalService = createAbsenceRequestApproveService();

			request.Approve(approvalService, MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());
			return request;
		}

		private static AbsenceRequestApprovalService createAbsenceRequestApproveService()
		{
			var scenario = ScenarioFactory.CreateScenario("Default", true, false);
			var dateTimePeriod = new DateTimePeriod(2010, 1, 1, 2010, 1, 2);
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, dateTimePeriod);
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var businessRules = new FakeNewBusinessRuleCollection();
			businessRules.SetRuleResponse(new List<IBusinessRuleResponse>
			{
				new BusinessRuleResponse(typeof(BusinessRuleResponse), "warning", true, false, dateTimePeriod, person,
					new DateOnlyPeriod(2010, 1, 1, 2010, 1, 2), "test warning")
			});
			var scheduleDayChangeCallback = new DoNothingScheduleDayChangeCallBack();
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			return new AbsenceRequestApprovalService(scenario, scheduleDictionary, businessRules, scheduleDayChangeCallback,
				globalSettingDataRepository, new CheckingPersonalAccountDaysProvider(personAbsenceAccountRepository));
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLinkProvider>().For<ILinkProvider>();
			isolate.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
		}
	}
}
