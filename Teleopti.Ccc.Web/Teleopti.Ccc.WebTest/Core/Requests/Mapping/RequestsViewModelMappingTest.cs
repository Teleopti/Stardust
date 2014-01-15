using System;
using System.Collections.Generic;
using System.Globalization;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
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
		private IUserCulture _userCulture;

		[SetUp]
		public void Setup()
		{
			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_userTimeZone.Stub(x => x.TimeZone()).Return(TimeZoneInfo.Local);
			_linkProvider = MockRepository.GenerateMock<ILinkProvider>();
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnPerson = PersonFactory.CreatePerson("LoggedOn", "Agent");
			_loggedOnUser.Expect(l => l.CurrentUser()).Return(_loggedOnPerson).Repeat.Any();
			_shiftTradeRequestStatusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
			_userCulture = MockRepository.GenerateMock<IUserCulture>();
			_userCulture.Stub(x => x.GetCulture()).Return(CultureInfo.CurrentCulture);

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new RequestsViewModelMappingProfile(
				_userTimeZone, 
				_linkProvider,
				_loggedOnUser,
				_shiftTradeRequestStatusChecker,
				_userCulture
				)));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapLink()
		{
			var request = new PersonRequest(new Person());
			request.SetId(Guid.NewGuid());

			_linkProvider.Stub(x => x.RequestDetailLink(request.Id.Value)).Return("aLink");

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Link.rel.Should().Be("self");
			result.Link.href.Should().Be("aLink");
			result.Link.Methods.Should().Contain("GET");
			result.Link.Methods.Should().Contain("DELETE");
			result.Link.Methods.Should().Contain("PUT");
		}

		[Test]
		public void ShouldNotMapLinksDeleteMethodIfStateApproved()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Approve(MockRepository.GenerateMock<IRequestApprovalService>(), MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());
			
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Link.Methods.Should().Not.Contain("DELETE");
		}

		[Test]
		public void ShouldMapPayload()
		{
			const string payLoadName = "this is the one";
			var abs = new Absence {Description = new Description(payLoadName)};
			var request = new PersonRequest(new Person(), new AbsenceRequest(abs, new DateTimePeriod(1900, 1, 1, 1900, 1, 2)));
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);
			result.Payload.Should().Be.EqualTo(payLoadName);
		}

		[Test]
		public void ShouldNotMapPayload()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);
			result.Payload.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotMapLinksDeleteMethodIfStateDenied()
		{
			var request = new PersonRequest(new Person());
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Deny(null, null, MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Link.Methods.Should().Not.Contain("DELETE");
		}

		[Test]
		public void ShouldNotMapLinksPutMethodIfStateApproved()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Approve(MockRepository.GenerateMock<IRequestApprovalService>(), MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Link.Methods.Should().Not.Contain("PUT");
		}

		[Test]
		public void ShouldNotMapLinksPutMethodIfStateDenied()
		{
			var request = new PersonRequest(new Person());
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Deny(null, null, MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Link.Methods.Should().Not.Contain("PUT");
		}

		[Test]
		public void ShouldMapId()
		{
			var request = new PersonRequest(new Person()) { Subject = "Test" };
			request.SetId(Guid.NewGuid());

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Id.Should().Be(request.Id.ToString());
		}

		[Test]
		public void ShouldMapSubject()
		{
			var request = new PersonRequest(new Person()){Subject = "Test"};

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Subject.Should().Be("Test");
		}

		[Test]
		public void ShouldMapDatesForOneDate()
		{
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var startDateTime = DateTime.UtcNow;
			var period = new DateTimePeriod(startDateTime, startDateTime.AddHours(5));
			
			var request = new PersonRequest(new Person(), new TextRequest(period));

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Dates.Should().Be.EqualTo(startDateTime.ToShortDateString());
		}

		[Test]
		public void ShouldMapDatesForDatePeriod()
		{
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var startDateTime = DateTime.UtcNow;
			var period = new DateTimePeriod(startDateTime, startDateTime.AddDays(5));

			var request = new PersonRequest(new Person(), new TextRequest(period));

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Dates.Should().Be.EqualTo(period.ToDateOnlyPeriod(_userTimeZone.TimeZone()).ToShortDateString(_userCulture.GetCulture()));
		}

		[Test]
		public void ShouldMapType()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Type.Should().Be(request.Request.RequestTypeDescription);
		}

		[Test]
		public void ShouldMapRequestTypeEnum()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.TypeEnum.Should().Be(RequestType.TextRequest);
		}

		[Test]
		public void ShouldMapUpdatedOn()
		{
			var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod())) {UpdatedOn = DateTime.UtcNow};

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.UpdatedOn.Should().Be(TimeZoneInfo.ConvertTimeFromUtc(request.UpdatedOn.Value, timeZone).ToShortDateTimeString());
		}

		[Test]
		public void ShouldMapText()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.TrySetMessage("Message");
			
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Text.Should().Be.EqualTo("Message");
		}

		[Test]
		public void ShouldMapStatus()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Status.Should().Be.EqualTo(request.StatusText);
		}

		[Test]
		public void ShouldMapRawTimeInfo()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 3, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var request = new PersonRequest(new Person(), new TextRequest(period));
			var startInCorrectTimezone = TimeZoneHelper.ConvertFromUtc(start, _userTimeZone.TimeZone());
			var endInCorrectTimezone = TimeZoneHelper.ConvertFromUtc(end, _userTimeZone.TimeZone());

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.RawTimeFrom.Should().Be.EqualTo(startInCorrectTimezone.ToShortTimeString());
			result.RawTimeTo.Should().Be.EqualTo(endInCorrectTimezone.ToShortTimeString());
		}

		[Test, SetCulture("ar-SA")]
		public void ShouldMapYearMonthAndDayParts()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var start = new DateTime(2013, 3, 18, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2013, 3, 19, 0, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var request = new PersonRequest(new Person(), new TextRequest(period));

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			var calendar = CultureInfo.CurrentCulture.Calendar;
			var dateFrom = TimeZoneInfo.ConvertTimeFromUtc(request.Request.Period.StartDateTime, timeZone);
			var dateTo = TimeZoneInfo.ConvertTimeFromUtc(request.Request.Period.EndDateTime, timeZone);

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
			var request = new PersonRequest(new Person(), new AbsenceRequest(new Absence(),  new DateTimePeriod()));
			request.Deny(null, "RequestDenyReasonNoWorkflow", new PersonRequestAuthorizationCheckerForTest());

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.DenyReason.Should().Be.EqualTo(Resources.RequestDenyReasonNoWorkflow);
		}

		[Test]
		public void ShouldMapAlreadyTranslatedDenyReason()
		{
			var denyReason = "You must work at 2011-09-23.";
			var request = new PersonRequest(new Person(), new AbsenceRequest(new Absence(), new DateTimePeriod()));
			request.Deny(null, denyReason, new PersonRequestAuthorizationCheckerForTest());

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.DenyReason.Should().Be.EqualTo(denyReason);
		}

		[Test]
		public void ShouldSetIsCreatedByUserToTrueIfTheSenderIsTheSamePersonAsTheLoggedOnUser()
		{
			var receiver = PersonFactory.CreatePerson("Receiver");
			var personRequest = createShiftTrade(_loggedOnPerson, receiver);

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

			Assert.That(result.IsCreatedByUser, Is.True);
		}

		[Test]
		public void ShouldSetIsCreatedByUserToFalseIfTheSenderIsTheSamePersonAsTheLoggedOnUser()
		{
			var sender = PersonFactory.CreatePerson("Sender");
			var personRequest = createShiftTrade(sender, _loggedOnPerson);

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
			
			Assert.That(result.IsCreatedByUser,Is.False);
		}

		[Test]
		public void ShouldMapFrom()
		{
			var sender = PersonFactory.CreatePerson("Sender");
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, _loggedOnPerson, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(_loggedOnPerson) { Subject = "Subject of request", Request = shiftTradeRequest };

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

			Assert.That(result.From, Is.EqualTo(sender.Name.ToString()));
		}

		[Test]
		public void ShouldMapTo()
		{
			var sender = PersonFactory.CreatePerson("Sender");
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, _loggedOnPerson, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(_loggedOnPerson) { Subject = "Subject of request", Request = shiftTradeRequest };

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

			Assert.That(result.To, Is.EqualTo(_loggedOnPerson.Name.ToString()));
		}

		[Test]
		public void ShouldMapFromAndToToEmptyStringIfNotShiftTradeRequest()
		{
			var personRequest = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

			result.From.Should().Be.Empty();
			result.To.Should().Be.Empty();
		}

		[Test]
		public void ShouldMapShiftTradeStatusOkByMeWhenUserHasCreatedTheShiftTradeRequest()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			str.Expect(c => c.GetShiftTradeStatus(_shiftTradeRequestStatusChecker)).Return(ShiftTradeStatus.OkByMe);
			str.Expect(c => c.PersonFrom).Return(_loggedOnPerson);
			var personRequest = new PersonRequest(_loggedOnPerson, str);
			personRequest.ForcePending();
			
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
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
			
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
			result.Status.Should().Contain(Resources.WaitingForYourApproval);
		}

		[Test]
		public void ShouldMapShiftTradeStatusOkByBothParts()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			str.Expect(c => c.GetShiftTradeStatus(_shiftTradeRequestStatusChecker)).Return(ShiftTradeStatus.OkByBothParts);
			var personRequest = new PersonRequest(new Person(), str);
			personRequest.ForcePending();

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
			result.Status.Should().Contain(Resources.WaitingForSupervisorApproval);
		}

		[Test]
		public void ShouldOnlyMapShiftTradeStatusWhenPending()
		{
			var str = MockRepository.GenerateMock<IShiftTradeRequest>();
			var personRequest = new PersonRequest(new Person(), str);

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
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

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
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

			Assert.Throws<AutoMapperMappingException>(() =>
					Mapper.Map<IPersonRequest, RequestViewModel>(personRequest));
		}

		[Test]
		public void ShouldMapIsNew()
		{
			var sender = PersonFactory.CreatePerson();
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(sender, _loggedOnPerson, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(_loggedOnPerson) { Subject = "Subject of request", Request = shiftTradeRequest };
			
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

			Assert.That(personRequest.IsNew);
			Assert.That(result.IsNew, Is.True);

			shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.Referred, new PersonRequestAuthorizationCheckerForTest());
			result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

			Assert.That(personRequest.IsNew,Is.False);
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
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

			Assert.That(personRequest.IsNew);
			Assert.That(result.IsPending, Is.False);

			personRequest.ForcePending();			
			result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

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
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

			Assert.That(personRequest.IsApproved, Is.False); 
			Assert.That(result.IsApproved, Is.False);

			personRequest.ForcePending();
			personRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());
			result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

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
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

			Assert.That(personRequest.IsDenied, Is.False);
			Assert.That(result.IsApproved, Is.False);

			personRequest.ForcePending();
			personRequest.Deny(null, "MyDenyReason", new PersonRequestAuthorizationCheckerForTest());
			result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

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
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

			Assert.That(result.IsReferred, Is.False);

			shiftTradeRequest.Refer(new PersonRequestAuthorizationCheckerForTest());
			result = Mapper.Map<IPersonRequest, RequestViewModel>(personRequest);

			Assert.That(result.IsReferred, Is.True);
		}

		private static IPersonRequest createShiftTrade(IPerson from, IPerson to)
		{
			var tradeDate = new DateOnly(2010, 1, 1);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(from, to, tradeDate, tradeDate);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(from) { Subject = "Subject of request", Request = shiftTradeRequest };
			personRequest.TrySetMessage("This is a short text for the description of a shift trade request");
			personRequest.SetId(new Guid());
			return personRequest;
		}
	}
}
