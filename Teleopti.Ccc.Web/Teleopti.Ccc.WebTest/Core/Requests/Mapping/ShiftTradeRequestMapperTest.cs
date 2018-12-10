using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;


namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeRequestMapperTest
	{
		private IShiftTradeRequestMapper _target;
		private IPersonRepository personRepository;
		private ShiftTradeRequestForm form;
		private IPerson loggedOnUser;
		private ILoggedOnUser _loggedOnUserSvc;
		private IPersonRequestRepository _personRequestRepository;
		private IScheduleProvider _scheduleProvider;
		private IScheduleDay _scheduleToTrade;

		[SetUp]
		public void Setup()
		{
			loggedOnUser = PersonFactory.CreatePerson();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_loggedOnUserSvc = MockRepository.GenerateMock<ILoggedOnUser>();
			_personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			_scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			_target = new ShiftTradeRequestMapper(personRepository, _loggedOnUserSvc, _personRequestRepository, _scheduleProvider);
			form = new ShiftTradeRequestForm
				{
					Message = "sdfsdfsdf",
					Subject = "sdfsdff",
					PersonToId = Guid.NewGuid(),
					Dates = new List<DateOnly> { new DateOnly(2000, 1, 1) }
				};
			personRepository.Stub(x => x.Get(form.PersonToId)).Return(new Person());
			_loggedOnUserSvc.Stub(x => x.CurrentUser()).Return(loggedOnUser);
			_scheduleToTrade = ScheduleDayFactory.Create(form.Dates.SingleOrDefault());
			_scheduleProvider.Stub(x => x.GetScheduleForPersons(form.Dates.SingleOrDefault(), new[] { loggedOnUser })).Return(new[] { _scheduleToTrade });	
		}

		[Test]
		public void ShouldMapSubject()
		{
			const string expected = "hejhej";
			form.Subject = expected;
			
			var res = _target.Map(form);
			res.GetSubject(new NoFormatting()).Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldMapPersonRequestFromOffer()
		{
			const string expected = "hejhej";
			form.Subject = expected;
			var offerId = Guid.Empty;
			form.ShiftExchangeOfferId = offerId;
			var offer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			var offerRequest = new PersonRequest(PersonFactory.CreatePerson()){Request = offer};
			_personRequestRepository.Stub(x => x.FindPersonRequestByRequestId(offerId)).Return(offerRequest);

			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			offer.Stub(x => x.MakeShiftTradeRequest(_scheduleToTrade)).Return(personRequest);
			
			var res = _target.Map(form);
			res.Should().Be.SameInstanceAs(personRequest);
			personRequest.AssertWasCalled(x => x.Subject = expected);
		}

		[Test]
		public void ShouldMapGetMessage()
		{
			const string expected = "hej då";
			form.Message = expected;

			var res = _target.Map(form);
			res.GetMessage(new NoFormatting()).Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldMapPerson()
		{
			var res = _target.Map(form);
			res.Person.Should().Be.SameInstanceAs(loggedOnUser);
		}

		[Test]
		public void ShouldMapShiftTradeRequestWithCorrectDate()
		{
			var expected = new DateOnly(2010, 1, 1);
			form.Dates = new List<DateOnly> {expected};

			var res = _target.Map(form);
			var swapDetail = ((IShiftTradeRequest) res.Request).ShiftTradeSwapDetails[0];
			swapDetail.DateFrom.Should().Be.EqualTo(expected);
			swapDetail.DateTo.Should().Be.EqualTo(expected);
		}

		[Test, SetCulture("ar-SA")]
		public void ShouldMapShiftTradeRequestWithCorrectDateForArabicCulture()
		{
			form.Dates = new List<DateOnly> {new DateOnly(1434, 5, 1)};
			var expected = new DateOnly(new DateTime(1434, 5, 1, CultureInfo.CurrentCulture.Calendar));

			var res = _target.Map(form);
			var swapDetail = ((IShiftTradeRequest)res.Request).ShiftTradeSwapDetails[0];
			swapDetail.DateFrom.Should().Be.EqualTo(expected);
			swapDetail.DateTo.Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldMapShiftTradeRequestWithCorrectPersons()
		{
			var personTo = new Person();
			var id = Guid.NewGuid();
			personRepository.Expect(x => x.Get(id)).Return(personTo);
			form.PersonToId = id;

			var res = _target.Map(form);
			var swapDetail = ((IShiftTradeRequest)res.Request).ShiftTradeSwapDetails[0];
			swapDetail.PersonTo.Should().Be.SameInstanceAs(personTo);
			swapDetail.PersonFrom.Should().Be.SameInstanceAs(loggedOnUser);
		}
	}
}