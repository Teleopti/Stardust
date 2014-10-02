using System;
using System.Security.Cryptography;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class ShareCalendarControllerTest
	{

		private ICalendarLinkIdGenerator _calendarLinkIdGenerator;
		private FakeHttpContext _context;
		private const string calendarlinkid = "calendarLinkId";

		[SetUp]
		public void Setup()
		{
			_calendarLinkIdGenerator = MockRepository.GenerateMock<ICalendarLinkIdGenerator>();

			var response = MockRepository.GenerateStub<FakeHttpResponse>();
			_context = new FakeHttpContext("/");
			_context.SetResponse(response);
		}

		[Test]
		public void ShouldGetCalendarForPerson()
		{
			var calendarLinkGenerator = MockRepository.GenerateMock<ICalendarLinkGenerator>();
			var calendarLinkId = new CalendarLinkId();
			calendarLinkGenerator.Stub(x => x.Generate(calendarLinkId)).Return("test");
			_calendarLinkIdGenerator.Stub(x => x.Parse(calendarlinkid)).Return(calendarLinkId);

			var target = new ShareCalendarController(_calendarLinkIdGenerator, calendarLinkGenerator);

			var result = target.iCal(calendarlinkid);
			result.Content.Should().Contain("test");
		}

		[Test]
		public void ShouldReturnErrorIfUrlInvalid1()
		{
			_calendarLinkIdGenerator.Stub(x => x.Parse(calendarlinkid)).Throw(new FormatException());
			var target = new ShareCalendarController(_calendarLinkIdGenerator, null);
			target.ControllerContext = new ControllerContext(_context, new RouteData(), target);

			var result = target.iCal(calendarlinkid);
			result.Content.Should().Contain("Invalid url");
		}

		[Test]
		public void ShouldReturnErrorIfUrlInvalid2()
		{
			_calendarLinkIdGenerator.Stub(x => x.Parse(calendarlinkid)).Throw(new CryptographicException());
			var target = new ShareCalendarController(_calendarLinkIdGenerator, null);
			target.ControllerContext = new ControllerContext(_context, new RouteData(), target);

			var result = target.iCal(calendarlinkid);
			result.Content.Should().Contain("Invalid url");
		}

		[Test]
		public void ShouldReturnErrorIfNoCalendarLinkPermission()
		{
			var calendarLinkId = new CalendarLinkId();
			_calendarLinkIdGenerator.Stub(x => x.Parse(calendarlinkid)).Return(calendarLinkId);
			var calendarLinkGenerator = MockRepository.GenerateMock<ICalendarLinkGenerator>();
			calendarLinkGenerator.Stub(x => x.Generate(calendarLinkId)).Throw(new PermissionException());
			var target = new ShareCalendarController(_calendarLinkIdGenerator, calendarLinkGenerator);
			target.ControllerContext = new ControllerContext(_context, new RouteData(), target);

			var result = target.iCal(calendarlinkid);
			result.Content.Should().Contain("No permission");
		}

		[Test]
		public void ShouldReturnErrorIfCalendarLinkNonActive()
		{
			var calendarLinkId = new CalendarLinkId();
			_calendarLinkIdGenerator.Stub(x => x.Parse(calendarlinkid)).Return(calendarLinkId);
			var calendarLinkGenerator = MockRepository.GenerateMock<ICalendarLinkGenerator>();
			calendarLinkGenerator.Stub(x => x.Generate(calendarLinkId)).Throw(new InvalidOperationException());

			var target = new ShareCalendarController(_calendarLinkIdGenerator, calendarLinkGenerator);
			target.ControllerContext = new ControllerContext(_context, new RouteData(), target);
			var result = target.iCal(calendarlinkid);
			result.Content.Should().Contain("inactive");
		}
	}
}