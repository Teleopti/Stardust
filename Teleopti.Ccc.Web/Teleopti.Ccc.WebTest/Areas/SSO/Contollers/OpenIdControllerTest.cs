using System;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System.Web;
using System.Web.Routing;
using DotNetOpenAuth.OpenId.Provider;
using MvcContrib.TestHelper.Fakes;
using Teleopti.Ccc.Web.Areas.SSO.Controllers;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Areas.SSO.Contollers
{
	[TestFixture]
	public class OpenIdControllerTest
	{
		[Test]
		public void IdentifierShouldReturnXrds()
		{
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://mock/"), new Uri("http://mock/"));
			request.Stub(x => x.AcceptTypes).Return(new[] { "application/xrds+xml" });
			var context = new FakeHttpContext("/");
			context.SetRequest(request);
			var target = new OpenIdController(null, null, null);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			var result = target.Identifier();

			(result as ViewResult).ViewName.Should().Be.EqualTo("Xrds");
		}

		[Test]
		public void IdentifierShouldReturnView()
		{
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://mock/"), new Uri("http://mock/"));
			var context = new FakeHttpContext("/");
			context.SetRequest(request);
			var target = new OpenIdController(null, null, null);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			var result = target.Identifier();

			(result as ViewResult).ViewName.Should().Be.EqualTo("");
		}

		[Test]
		public void ProviderShouldAuthenticateMeIfImAWindowsUser()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWapper>();
			var request = MockRepository.GenerateMock<IAuthenticationRequest>();
			request.Stub(x => x.IsResponseReady).Return(false);
			request.Expect(x => x.IsAuthenticated).PropertyBehavior();
			request.Expect(x => x.LocalIdentifier).PropertyBehavior();
			openIdProviderWapper.Stub(x => x.GetRequest()).Return(request);

			var windowsAccountProvider = MockRepository.GenerateMock<IWindowsAccountProvider>();
			windowsAccountProvider.Stub(x => x.RetrieveWindowsAccount()).Return(new WindowsAccount("domainName", "userName"));

			var httpResponse = MockRepository.GenerateStub<HttpResponseBase>();
			httpResponse.Stub(x => x.ApplyAppPathModifier("~/SSO/OpenId/AskUser/userName@domainName")).Return("/SSO/OpenId/AskUser/userName@domainName");
			var httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
			httpRequest.Stub(x => x.Url).Return(new Uri("http://mock"));
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			var httpContext = MockRepository.GenerateStub<HttpContextBase>();
			httpContext.Stub(x => x.Response).Return(httpResponse);
			httpContext.Stub(x => x.Request).Return(httpRequest);
			currentHttpContext.Stub(x => x.Current()).Return(httpContext);

			var target = new OpenIdController(openIdProviderWapper, windowsAccountProvider, currentHttpContext);
			target.Provider();

			request.IsAuthenticated.Should().Be.EqualTo(true);
			request.LocalIdentifier.ToString().Should().Be("http://mock/SSO/OpenId/AskUser/userName@domainName");
			openIdProviderWapper.AssertWasCalled(x => x.SendResponse(request));
		}

		[Test]
		public void ProviderShouldNotAuthenticateMeIfImNotAWindowsUser()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWapper>();
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			var request = MockRepository.GenerateMock<IAuthenticationRequest>();
			request.Stub(x => x.IsResponseReady).Return(false);
			request.Expect(x => x.IsAuthenticated).PropertyBehavior();
			openIdProviderWapper.Stub(x => x.GetRequest()).Return(request);
			var windowsAccountProvider = MockRepository.GenerateMock<IWindowsAccountProvider>();
			windowsAccountProvider.Stub(x => x.RetrieveWindowsAccount()).Return(null);

			var target = new OpenIdController(openIdProviderWapper, windowsAccountProvider, currentHttpContext);
			target.Provider();

			request.IsAuthenticated.Should().Be.EqualTo(false);
			openIdProviderWapper.AssertWasNotCalled(x => x.SendResponse(request));
		}

		[Test]
		public void ProviderShouldSimplyRespondIfTheResponseIsReady()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWapper>();
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			var request = MockRepository.GenerateMock<IAuthenticationRequest>();
			request.Stub(x => x.IsResponseReady).Return(true);
			openIdProviderWapper.Stub(x => x.GetRequest()).Return(request);
			var outgoingWebResponse = MockRepository.GenerateMock<OutgoingWebResponse>();
			openIdProviderWapper.Stub(x => x.PrepareResponse(request)).Return(outgoingWebResponse);
			var windowsAccountProvider = MockRepository.GenerateMock<IWindowsAccountProvider>();

			var target = new OpenIdController(openIdProviderWapper, windowsAccountProvider, currentHttpContext);
			target.Provider();

			openIdProviderWapper.AssertWasCalled(x => x.PrepareResponse(request));
			openIdProviderWapper.AssertWasNotCalled(x => x.SendResponse(request));
		}

		[Test]
		public void ShouldReturnAskUserView()
		{
			var target = new OpenIdController(null, null, null);

			var result = target.AskUser();

			(result as ViewResult).ViewName.Should().Be.EqualTo("");
		}
		
	}
}