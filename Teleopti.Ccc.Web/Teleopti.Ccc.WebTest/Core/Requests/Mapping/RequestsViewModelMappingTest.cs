using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class RequestsViewModelMappingTest
	{
		private IUserTimeZone _userTimeZone;
		private ILinkProvider _linkProvider;
		private ILoggedOnUser _loggedOnUser;
		private IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
		private IPerson _loggedOnPerson;
		private IPersonNameProvider _personNameProvider;
		private TimeZoneInfo _timeZone;
		private IToggleManager _toggleManager;
		private RequestsViewModelMapper target;

		[SetUp]
		public void Setup()
		{
			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_userTimeZone.Stub(x => x.TimeZone()).Return(_timeZone);

			_linkProvider = MockRepository.GenerateMock<ILinkProvider>();
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();

			_loggedOnPerson = PersonFactory.CreatePerson("LoggedOn", "Agent");
			_loggedOnUser.Expect(l => l.CurrentUser()).Return(_loggedOnPerson).Repeat.Any();
			_shiftTradeRequestStatusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();

			_personNameProvider = MockRepository.GenerateMock<IPersonNameProvider>();
			_personNameProvider.Stub(x => x.BuildNameFromSetting(_loggedOnUser.CurrentUser().Name,null)).IgnoreArguments().Return("LoggedOn Agent");

			_toggleManager = new TrueToggleManager();
			
			target = new RequestsViewModelMapper(_userTimeZone,_linkProvider,_loggedOnUser,_shiftTradeRequestStatusChecker,_personNameProvider,_toggleManager);
		}
		
		[Test]
		public void ShouldMapLink()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.SetId(Guid.NewGuid());

			_linkProvider.Stub(x => x.RequestDetailLink(request.Id.Value)).Return("aLink");

			var result = target.Map(request);

			result.Link.rel.Should().Be("self");
			result.Link.href.Should().Be("aLink");
			result.Link.Methods.Should().Contain("GET");
			result.Link.Methods.Should().Contain("DELETE");
			result.Link.Methods.Should().Contain("PUT");
		}

		[Test]
		public void ShouldMapLinkForOfferStatusPending()
		{
			var result = stubRequestLinkForShiftExchangeOffer(ShiftExchangeOfferStatus.Pending, false);

			result.Link.rel.Should().Be("self");
			result.Link.href.Should().Be("aLink");
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
			result.Link.href.Should().Be("aLink");
			result.Link.Methods.Should().Contain("GET");
			result.Link.Methods.Should().Contain("DELETE");
			result.Link.Methods.Should().Not.Contain("PUT");
		}

		private RequestViewModel stubRequestLinkForShiftExchangeOffer(ShiftExchangeOfferStatus status, bool isExpired)
		{
			var offer = createShiftExchangeOffer(status, isExpired);
			var request = new PersonRequest(new Person(), offer);
			request.SetId(Guid.NewGuid());

			_linkProvider.Stub(x => x.RequestDetailLink(request.Id.Value)).Return("aLink");

			var result = target.Map(request);
			return result;
		}

		[Test]
		public void ShouldNotMapLinksDeleteMethodIfStateApproved()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Approve(MockRepository.GenerateMock<IRequestApprovalService>(), MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());

			var result = target.Map(request);

			result.Link.Methods.Should().Not.Contain("DELETE");
		}

		[Test]
		public void ShouldMapPayload()
		{
			const string payLoadName = "this is the one";
			var abs = new Absence { Description = new Description(payLoadName) };
			var request = new PersonRequest(new Person(), new AbsenceRequest(abs, new DateTimePeriod(1900, 1, 1, 1900, 1, 2)));
			var result = target.Map(request);
			result.Payload.Should().Be.EqualTo(payLoadName);
		}

		[Test]
		public void ShouldNotMapPayload()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			var result = target.Map(request);
			result.Payload.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotMapLinksDeleteMethodIfStateDenied()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Deny(null, MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());
			var result = target.Map(request);

			result.Link.Methods.Should().Not.Contain("DELETE");
		}

		[Test]
		public void ShouldNotMapLinksPutMethodIfStateApproved()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Approve(MockRepository.GenerateMock<IRequestApprovalService>(), MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());

			var result = target.Map(request);

			result.Link.Methods.Should().Not.Contain("PUT");
		}

		[Test]
		public void ShouldNotMapLinksPutMethodIfStateDenied()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Deny(null, MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());
			var result = target.Map(request);

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
		public void ShouldNotMapCancelMethodIfToggleOff()
		{
			var result = setupForToggleCheckOnApprovedRequest(new FakeToggleManager());

			result.Link.Methods.Should().Not.Contain ("CANCEL");
		}

		[Test]
		public void ShouldMapCancelMethodIfToggleOn()
		{
			setupStateHolderProxy();

			var toggleManager = new FakeToggleManager(Toggles.Wfm_Requests_Cancel_Agent_38055);

			var result = setupForToggleCheckOnApprovedRequest(toggleManager);

			result.Link.Methods.Should().Contain("CANCEL");
		}

		
		[Test]
		public void ShouldMapId()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod())) { Subject = "Test" };
			request.SetId(Guid.NewGuid());

			var result = target.Map(request);

			result.Id.Should().Be(request.Id.ToString());
		}

		[Test]
		public void ShouldMapSubject()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod())) { Subject = "Test" };
			var result = target.Map(request);

			result.Subject.Should().Be("Test");
		}

		[Test]
		public void ShouldMapDate()
		{
			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(5));
			var request = new PersonRequest(new Person(), new TextRequest(period));
			var result = target.Map(request);

			result.DateTimeFrom.Should().Be.EqualTo(DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(period.StartDateTime, _timeZone));
			result.DateTimeTo.Should().Be.EqualTo(DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(period.EndDateTime, _timeZone));

			DateTime.Parse(result.DateTimeFrom).Kind.Should().Be(DateTimeKind.Unspecified);
			DateTime.Parse(result.DateTimeTo).Kind.Should().Be(DateTimeKind.Unspecified);

		}

		[Test]
		public void ShouldMapDatesForShiftRequestForOneDate()
		{
			_loggedOnPerson.PermissionInformation.SetDefaultTimeZone(_timeZone);

			var startDate = new DateOnly(DateTime.UtcNow);
			var request = createShiftTrade(_loggedOnPerson, PersonFactory.CreatePerson("Receiver"),
										   startDate, startDate);

			var result = target.Map(request);

			new DateOnly(DateTime.Parse(result.DateTimeFrom)).Should().Be.EqualTo(startDate);
			new DateOnly(DateTime.Parse(result.DateTimeTo)).Should().Be.EqualTo(startDate);

		}

		[Test]
		public void ShouldMapDatesForShiftRequestForDatePeriod()
		{
			_loggedOnPerson.PermissionInformation.SetDefaultTimeZone(_timeZone);

			var startDate = new DateOnly(DateTime.UtcNow);
			var endDate = startDate.AddDays(5);

			var request = createShiftTrade(_loggedOnPerson, PersonFactory.CreatePerson("Receiver"),
										   startDate, endDate);

			var result = target.Map(request);

			new DateOnly(DateTime.Parse(result.DateTimeFrom)).Should().Be.EqualTo(startDate);
			new DateOnly(DateTime.Parse(result.DateTimeTo)).Should().Be.EqualTo(endDate);


		}

		[Test]
		public void ShouldMapType()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			var result = target.Map(request);

			result.Type.Should().Be(request.Request.RequestTypeDescription);
		}

		[Test]
		public void ShouldMapRequestTypeEnum()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			var result = target.Map(request);

			result.TypeEnum.Should().Be(RequestType.TextRequest);
		}

		[Test]
		public void ShouldMapUpdatedOn()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod())) { UpdatedOn = DateTime.UtcNow };
			var result = target.Map(request);

			result.UpdatedOnDateTime.Should().Be(DateTimeMappingUtils.ConvertUtcToLocalDateTimeString(request.UpdatedOn.Value, _timeZone));

		}

		[Test]
		public void ShouldMapText()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.TrySetMessage("Message");

			var result = target.Map(request);

			result.Text.Should().Be.EqualTo("Message");
		}

		[Test]
		public void ShouldMapStatus()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			var result = target.Map(request);

			result.Status.Should().Be.EqualTo(request.StatusText);
		}

		[Test]
		public void ShouldMapRawTimeInfo()
		{
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 3, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var request = new PersonRequest(new Person(), new TextRequest(period));
			var startInCorrectTimezone = TimeZoneHelper.ConvertFromUtc(start, _userTimeZone.TimeZone());
			var endInCorrectTimezone = TimeZoneHelper.ConvertFromUtc(end, _userTimeZone.TimeZone());

			var result = target.Map(request);

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

			var result = target.Map(request);

			var calendar = CultureInfo.CurrentCulture.Calendar;
			var dateFrom = TimeZoneInfo.ConvertTimeFromUtc(request.Request.Period.StartDateTime, _timeZone);
			var dateTo = TimeZoneInfo.ConvertTimeFromUtc(request.Request.Period.EndDateTime, _timeZone);

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

			var result = target.Map(request);

			result.DenyReason.Should().Be.EqualTo(Resources.RequestDenyReasonNoWorkflow);
		}

		[Test]
		public void ShouldMapAlreadyTranslatedDenyReason()
		{
			var denyReason = "You must work at 2011-09-23.";
			var request = new PersonRequest(new Person(), new AbsenceRequest(new Absence(), new DateTimePeriod()));
			request.Deny( denyReason, new PersonRequestAuthorizationCheckerForTest());

			var result = target.Map(request);

			result.DenyReason.Should().Be.EqualTo(denyReason);
		}

		[Test]
		public void ShouldSetIsCreatedByUserToTrueIfTheSenderIsTheSamePersonAsTheLoggedOnUser()
		{
			var receiver = PersonFactory.CreatePerson("Receiver");
			var personRequest = createShiftTrade(_loggedOnPerson, receiver);

			var result = target.Map(personRequest);

			Assert.That(result.IsCreatedByUser, Is.True);
		}

		[Test]
		public void ShouldSetIsCreatedByUserToFalseIfTheSenderIsTheSamePersonAsTheLoggedOnUser()
		{
			var sender = PersonFactory.CreatePerson("Sender");
			var personRequest = createShiftTrade(sender, _loggedOnPerson);

			var result = target.Map(personRequest);

			Assert.That(result.IsCreatedByUser, Is.False);
		}

		[Test]
		public void ShouldMapFrom()
		{
			var sender = PersonFactory.CreatePerson("Sender");
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, _loggedOnPerson, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(_loggedOnPerson) { Subject = "Subject of request", Request = shiftTradeRequest };

			_personNameProvider = MockRepository.GenerateMock<IPersonNameProvider>();
			_personNameProvider.Stub(x => x.BuildNameFromSetting(sender.Name,null)).IgnoreArguments().Return("Sender");

			target = new RequestsViewModelMapper(_userTimeZone,_linkProvider,_loggedOnUser,_shiftTradeRequestStatusChecker,_personNameProvider,new TrueToggleManager());
			var result = target.Map(personRequest);

			Assert.That(result.From, Is.EqualTo(sender.Name.FirstName));
		}

		[Test]
		public void ShouldMapTo()
		{
			var sender = PersonFactory.CreatePerson("Sender");
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, _loggedOnPerson, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(_loggedOnPerson) { Subject = "Subject of request", Request = shiftTradeRequest };

			var result = target.Map(personRequest);

			Assert.That(result.To, Is.EqualTo(_loggedOnPerson.Name.FirstName + " " + _loggedOnPerson.Name.LastName));
		}

		[Test]
		public void ShouldMapFromAndToToEmptyStringIfNotShiftTradeRequest()
		{
			var personRequest = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));

			var result = target.Map(personRequest);

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

			var personRequest = new PersonRequest(_loggedOnPerson, shiftExchangeOffer);
			var result = target.Map(personRequest);

			result.Status.Should().Contain(Resources.Expired);
		}

		[Test]
		public void ShouldMapShiftExchangeOfferNextDayTrue()
		{
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
			var personRequest = new PersonRequest(_loggedOnPerson, new ShiftTradeRequest(new List<IShiftTradeSwapDetail>()));
			
			var result = target.Map(personRequest);

			result.ExchangeOffer.IsOfferAvailable.Should().Be.True();
		}

		[Test]
		public void ShouldMapShiftTradeStatusOkByMeWhenUserHasCreatedTheShiftTradeRequest()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			str.Expect(c => c.GetShiftTradeStatus(_shiftTradeRequestStatusChecker)).Return(ShiftTradeStatus.OkByMe);
			str.Expect(c => c.PersonFrom).Return(_loggedOnPerson);
			var personRequest = new PersonRequest(_loggedOnPerson, str);
			personRequest.ForcePending();

			var result = target.Map(personRequest);
			result.Status.Should().Contain(Resources.WaitingForOtherPart);
		}

		[Test]
		public void ShouldMapShiftTradeStatusOkByMeWhenUserHasRecievedTheShiftTradeRequest()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			str.Expect(c => c.GetShiftTradeStatus(_shiftTradeRequestStatusChecker)).Return(ShiftTradeStatus.OkByMe);
			str.Expect(c => c.PersonFrom).Return(new Person());
			var personRequest = new PersonRequest(_loggedOnPerson, str);
			personRequest.ForcePending();

			var result = target.Map(personRequest);
			result.Status.Should().Contain(Resources.WaitingForYourApproval);
		}

		[Test]
		public void ShouldMapShiftTradeStatusOkByBothParts()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			str.Expect(c => c.GetShiftTradeStatus(_shiftTradeRequestStatusChecker)).Return(ShiftTradeStatus.OkByBothParts);
			var personRequest = new PersonRequest(new Person(), str);
			personRequest.ForcePending();

			var result = target.Map(personRequest);
			result.Status.Should().Contain(Resources.WaitingForSupervisorApproval);
		}

		[Test]
		public void ShouldOnlyMapShiftTradeStatusWhenPending()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			var personRequest = new PersonRequest(new Person(), str);

			var result = target.Map(personRequest);
			result.Status.Should().Not.Contain(",");

			str.AssertWasNotCalled(x => x.GetShiftTradeStatus(_shiftTradeRequestStatusChecker));
		}

		[Test]
		public void ShouldMapShiftTradeStatusReferred()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			str.Expect(c => c.GetShiftTradeStatus(_shiftTradeRequestStatusChecker)).Return(ShiftTradeStatus.Referred);
			var personRequest = new PersonRequest(new Person(), str);
			personRequest.ForcePending();

			var result = target.Map(personRequest);
			result.Status.Should().Contain(Resources.TheScheduleHasChanged);
		}

		[Test]
		public void ShouldThrowShiftTradeStatusNotValid()
		{
			//AFAIK - this status is not in used. Cannot be deleted due to SDK thingys (?).
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			str.Expect(c => c.GetShiftTradeStatus(_shiftTradeRequestStatusChecker)).Return(ShiftTradeStatus.NotValid);
			var personRequest = new PersonRequest(new Person(), str);
			personRequest.ForcePending();

			Assert.Throws<ArgumentException>(() =>
					target.Map(personRequest));
		}

		[Test]
		public void ShouldMapIsNew()
		{
			var sender = PersonFactory.CreatePerson();
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, _loggedOnPerson, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(_loggedOnPerson) { Subject = "Subject of request", Request = shiftTradeRequest };

			var result = target.Map(personRequest);

			Assert.That(personRequest.IsNew);
			Assert.That(result.IsNew, Is.True);

			shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.Referred, new PersonRequestAuthorizationCheckerForTest());
			result = target.Map(personRequest);

			Assert.That(personRequest.IsNew, Is.False);
			Assert.That(result.IsNew, Is.False);

		}

		[Test]
		public void ShouldMapIsPending()
		{
			var sender = PersonFactory.CreatePerson();
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, _loggedOnPerson, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(_loggedOnPerson) { Subject = "Subject of request", Request = shiftTradeRequest };
			var result = target.Map(personRequest);

			Assert.That(personRequest.IsNew);
			Assert.That(result.IsPending, Is.False);

			personRequest.ForcePending();
			result = target.Map(personRequest);

			Assert.That(personRequest.IsPending, Is.True);
			Assert.That(result.IsPending, Is.True);
		}

		[Test]
		public void ShouldMapIsApproved()
		{
			var sender = PersonFactory.CreatePerson();
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, _loggedOnPerson, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(_loggedOnPerson) { Subject = "Subject of request", Request = shiftTradeRequest };
			var result = target.Map(personRequest);

			Assert.That(personRequest.IsApproved, Is.False);
			Assert.That(result.IsApproved, Is.False);

			personRequest.ForcePending();
			personRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());
			result = target.Map(personRequest);

			Assert.That(personRequest.IsApproved, Is.True);
			Assert.That(result.IsApproved, Is.True);
		}

		[Test]
		public void ShouldMapIsDenied()
		{
			var sender = PersonFactory.CreatePerson();
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, _loggedOnPerson, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(_loggedOnPerson) { Subject = "Subject of request", Request = shiftTradeRequest };
			var result = target.Map(personRequest);

			Assert.That(personRequest.IsDenied, Is.False);
			Assert.That(result.IsApproved, Is.False);

			personRequest.ForcePending();
			personRequest.Deny("MyDenyReason", new PersonRequestAuthorizationCheckerForTest());
			result = target.Map(personRequest);

			Assert.That(personRequest.IsDenied, Is.True);
			Assert.That(result.IsDenied, Is.True);
		}

		[Test]
		public void ShouldMapIsReferred()
		{
			var sender = PersonFactory.CreatePerson();
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, _loggedOnPerson, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(_loggedOnPerson) { Subject = "Subject of request", Request = shiftTradeRequest };
			var result = target.Map(personRequest);

			Assert.That(result.IsReferred, Is.False);

			shiftTradeRequest.Refer(new PersonRequestAuthorizationCheckerForTest());
			result = target.Map(personRequest);

			Assert.That(result.IsReferred, Is.True);
		}

		[Test]
		public void ShouldHandleEmptyShiftSwapDetail()
		{
			var shiftTradeSwapDetails = new List<IShiftTradeSwapDetail>();
			var shifTradeRequest = new ShiftTradeRequest(shiftTradeSwapDetails);
			var personRequest = new PersonRequest(PersonFactory.CreatePerson(), shifTradeRequest);
			var shiftTradeRequestModel = target.Map(personRequest);

			Assert.That(shiftTradeRequestModel.From, Is.Null.Or.Empty);
			Assert.That(shiftTradeRequestModel.To, Is.Null.Or.Empty);
		}

		[Test]
		public void ShouldMapMultiplicatorDefinitionSet()
		{
			var definitionSet =  MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("test", MultiplicatorType.Overtime).WithId();
			var overtimeRequest = new OvertimeRequest(definitionSet, new DateTimePeriod());
			var personRequest = new PersonRequest(PersonFactory.CreatePerson(), overtimeRequest);

			var overtimeRequestModel = target.Map(personRequest);

			Assert.AreEqual(overtimeRequestModel.MultiplicatorDefinitionSet, definitionSet.Id.ToString());
		}

		private static IPersonRequest createShiftTrade(IPerson from, IPerson to)
		{
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(from, to, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(from) { Subject = "Subject of request", Request = shiftTradeRequest };
			personRequest.TrySetMessage("This is a short text for the description of a shift trade request");
			personRequest.SetId(Guid.Empty);
			return personRequest;
		}

		private static IPersonRequest createShiftTrade(IPerson from, IPerson to, DateOnly fromDate, DateOnly toDate)
		{
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(from, to, fromDate, toDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(from) { Subject = "Subject of request", Request = shiftTradeRequest };
			personRequest.TrySetMessage("This is a short text for the description of a shift trade request");
			personRequest.SetId(Guid.Empty);
			return personRequest;
		}

		private RequestViewModel createRequestViewModelWithShiftExchangeOffer(ShiftExchangeOffer shiftExchangeOffer)
		{
			var personRequest = new PersonRequest(_loggedOnPerson, shiftExchangeOffer);
			var result = target.Map(personRequest);
			return result;

		}

		private ShiftExchangeOffer createShiftExchangeOffer(ShiftExchangeOfferStatus status, bool isExpired)
		{
			var currentShift = ScheduleDayFactory.Create(isExpired ? DateOnly.Today : DateOnly.Today.AddDays(1));
			var str = new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(), status);
			return str;
		}

		private ShiftExchangeOffer createShiftExchangeOffer(DateTimePeriod period)
		{
			var scheduleDayFilterCriteria = new ScheduleDayFilterCriteria(ShiftExchangeLookingForDay.WorkingShift, period);
			var periodStartDate = new DateOnly(period.StartDateTime);
			var shiftExchagneCriteria = new ShiftExchangeCriteria(periodStartDate.AddDays(-1), scheduleDayFilterCriteria);
			var currentShift = ScheduleDayFactory.Create(periodStartDate.AddDays(-2));
			return new ShiftExchangeOffer(currentShift, shiftExchagneCriteria, ShiftExchangeOfferStatus.Pending);
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

			request.Deny(null, MockRepository.GenerateMock<IPersonRequestCheckAuthorization>(), null, PersonRequestDenyOption.AutoDeny);

			return target.Map(request);
		}

		private static void setupStateHolderProxy()
		{
			var stateMock = new FakeState();
			var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);
			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
		}

		private RequestViewModel setupForToggleCheckOnApprovedRequest(FakeToggleManager toggleManager)
		{

			var dateOnlyPeriod = DateOnly.Today.ToDateOnlyPeriod();
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var person = new Person();
			var request = createApprovedAbsenceRequest(absence, dateOnlyPeriod, person);

			target = new RequestsViewModelMapper(_userTimeZone,_linkProvider,_loggedOnUser,_shiftTradeRequestStatusChecker,_personNameProvider,toggleManager);

			var result = target.Map(request);
			return result;
		}

		private static PersonRequest createApprovedAbsenceRequest(IAbsence absence, DateOnlyPeriod dateOnlyPeriod, IPerson person)
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
			businessRules.SetRuleResponse(new List<IBusinessRuleResponse> { new BusinessRuleResponse(typeof(BusinessRuleResponse), "warning", true, false, dateTimePeriod, person, new DateOnlyPeriod(2010, 1, 1, 2010, 1, 2), "test warning") });
			var scheduleDayChangeCallback = new DoNothingScheduleDayChangeCallBack();
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			return new AbsenceRequestApprovalService(scenario, scheduleDictionary, businessRules, scheduleDayChangeCallback, globalSettingDataRepository, new CheckingPersonalAccountDaysProvider(personAbsenceAccountRepository));
		}

	}
}
