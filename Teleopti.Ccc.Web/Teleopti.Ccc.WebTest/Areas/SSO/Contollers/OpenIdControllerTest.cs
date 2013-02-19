using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.SSO.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.SSO.Contollers
{
	[TestFixture]
	public class OpenIdControllerTest
	{
		[Test]
		public void ShouldReturnXrdsView()
		{
			HttpContext.Current = new HttpContext(
				new HttpRequest("mock", "http://mock", "mock"),
				new HttpResponse(new StringWriter()));

			var response = MockRepository.GenerateStub<FakeHttpResponse>();
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			request.Stub(x => x.AcceptTypes).Return(new string[] { "application/xrds+xml" });
			var context = new FakeHttpContext("/");
			context.SetResponse(response);
			context.SetRequest(request);
			var target = new OpenIdController();
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			var result = target.Identifier();

			(result as ViewResult).ViewName.Should().Be.EqualTo("Xrds");
		}

		[Test]
		public void ShouldReturnIdentifierView()
		{
			HttpContext.Current = new HttpContext(
				new HttpRequest("mock", "http://mock", "mock"),
				new HttpResponse(new StringWriter()));

			var response = MockRepository.GenerateStub<FakeHttpResponse>();
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var context = new FakeHttpContext("/");
			context.SetResponse(response);
			context.SetRequest(request);
			var target = new OpenIdController();
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			var result = target.Identifier();

			(result as ViewResult).ViewName.Should().Be.EqualTo("");
		}
		
	}
}