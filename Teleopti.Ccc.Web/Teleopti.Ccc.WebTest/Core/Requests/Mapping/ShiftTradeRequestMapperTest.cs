using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeRequestMapperTest
	{
		private IShiftTradeRequestMapper target;
		private IPersonRepository personRepository;
		private ShiftTradeRequestForm form;
		private IPerson loggedOnUser;

		[SetUp]
		public void Setup()
		{
			loggedOnUser = new Person();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var loggedOnUserSvc = MockRepository.GenerateMock<ILoggedOnUser>();
			target = new ShiftTradeRequestMapper(personRepository, loggedOnUserSvc);
			form = new ShiftTradeRequestForm
				{
					Message = "sdfsdfsdf",
					Subject = "sdfsdff",
					PersonToId = Guid.NewGuid(),
					Dates = new List<DateOnly> { new DateOnly(2000, 1, 1) }
				};
			personRepository.Expect(x => x.Get(form.PersonToId)).Return(new Person());
			loggedOnUserSvc.Expect(x => x.CurrentUser()).Return(loggedOnUser);
		}

		[Test]
		public void ShouldMapSubject()
		{
			const string expected = "hejhej";
			form.Subject = expected;
			
			var res = target.Map(form);
			res.GetSubject(new NoFormatting()).Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldMapGetMessage()
		{
			const string expected = "hej då";
			form.Message = expected;

			var res = target.Map(form);
			res.GetMessage(new NoFormatting()).Should().Be.EqualTo(expected);
		}

		[Test]
		public void ShouldMapPerson()
		{
			var res = target.Map(form);
			res.Person.Should().Be.SameInstanceAs(loggedOnUser);
		}

		[Test]
		public void ShouldMapShiftTradeRequestWithCorrectDate()
		{
			var expected = new DateOnly(2010, 1, 1);
			form.Dates = new List<DateOnly> {expected};

			var res = target.Map(form);
			var swapDetail = ((IShiftTradeRequest) res.Request).ShiftTradeSwapDetails[0];
			swapDetail.DateFrom.Should().Be.EqualTo(expected);
			swapDetail.DateTo.Should().Be.EqualTo(expected);
		}

		[Test, SetCulture("ar-SA")]
		public void ShouldMapShiftTradeRequestWithCorrectDateForArabicCulture()
		{
			form.Dates = new List<DateOnly> {new DateOnly(1434, 5, 1)};
			var expected = new DateOnly(new DateTime(1434, 5, 1, CultureInfo.CurrentCulture.Calendar));

			var res = target.Map(form);
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

			var res = target.Map(form);
			var swapDetail = ((IShiftTradeRequest)res.Request).ShiftTradeSwapDetails[0];
			swapDetail.PersonTo.Should().Be.SameInstanceAs(personTo);
			swapDetail.PersonFrom.Should().Be.SameInstanceAs(loggedOnUser);
		}
	}
}