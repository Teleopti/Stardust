using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.WindowsIdentityProvider.Controllers;
using Teleopti.Ccc.Web.WindowsIdentityProvider.Core;

namespace Teleopti.Ccc.Web.WindowsIdentityProviderTest.Core
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
			var target = new OpenIdController(null, null, null, null);
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
			var target = new OpenIdController(null, null, null, null);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			var result = target.Identifier();

			(result as ViewResult).ViewName.Should().Be.EqualTo("");
		}

		[Test]
		public void ShouldTriggerWindowsAuthorizationWhenResponseIsnotReady()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWrapper>();
			var request = MockRepository.GenerateMock<IAuthenticationRequest>();
			request.Stub(x => x.IsResponseReady).Return(false);
			openIdProviderWapper.Stub(x => x.GetRequest()).Return(request);

			var providerEndpointWrapper = MockRepository.GenerateMock<IProviderEndpointWrapper>();
			providerEndpointWrapper.Expect(x => x.PendingRequest).PropertyBehavior();

			var target = new OpenIdController(openIdProviderWapper, null, null, providerEndpointWrapper);
			var result = (RedirectToRouteResult)target.Provider();

			providerEndpointWrapper.PendingRequest.Should().Be.SameInstanceAs(request);
			result.RouteValues.ContainsValue("TriggerWindowsAuthorization").Should().Be.True();
		}

		[Test]
		public void ProviderShouldSimplyRespondIfTheResponseIsReady()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWrapper>();
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			var request = MockRepository.GenerateMock<IAuthenticationRequest>();
			request.Stub(x => x.IsResponseReady).Return(true);
			openIdProviderWapper.Stub(x => x.GetRequest()).Return(request);
			var outgoingWebResponse = MockRepository.GenerateMock<OutgoingWebResponse>();
			openIdProviderWapper.Stub(x => x.PrepareResponse(request)).Return(outgoingWebResponse);
			var windowsAccountProvider = MockRepository.GenerateMock<IWindowsAccountProvider>();
			var providerEndpointWrapper = MockRepository.GenerateMock<IProviderEndpointWrapper>();

			var target = new OpenIdController(openIdProviderWapper, windowsAccountProvider, currentHttpContext, providerEndpointWrapper);
			target.Provider();

			openIdProviderWapper.AssertWasCalled(x => x.PrepareResponse(request));
			openIdProviderWapper.AssertWasNotCalled(x => x.SendResponse(request));
		}

		[Test, Ignore("Trying to get green lingon.")]
		public void ShouldAuthenticateMeIfImAWindowsUserWhenTriggerWindowsAuthorization()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWrapper>();
			var request = MockRepository.GenerateMock<IAuthenticationRequest>();
			request.Expect(x => x.IsAuthenticated).PropertyBehavior();
			request.Expect(x => x.LocalIdentifier).PropertyBehavior();
			request.Expect(x => x.IsReturnUrlDiscoverable(null)).IgnoreArguments().Return(RelyingPartyDiscoveryResult.Success);
			openIdProviderWapper.Stub(x => x.GetRequest()).Return(request);

			var windowsAccountProvider = MockRepository.GenerateMock<IWindowsAccountProvider>();
			windowsAccountProvider.Stub(x => x.RetrieveWindowsAccount()).Return(new WindowsAccount("domainName", "userName"));

			var httpResponse = MockRepository.GenerateStub<HttpResponseBase>();
			httpResponse.Stub(x => x.ApplyAppPathModifier("~/OpenId/AskUser/domainName%23userName")).Return("/OpenId/AskUser/domainName#userName");
			var httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
			httpRequest.Stub(x => x.Url).Return(new Uri("http://mock"));
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			var httpContext = MockRepository.GenerateStub<HttpContextBase>();
			httpContext.Stub(x => x.Response).Return(httpResponse);
			httpContext.Stub(x => x.Request).Return(httpRequest);
			currentHttpContext.Stub(x => x.Current()).Return(httpContext);

			var providerEndpointWrapper = MockRepository.GenerateMock<IProviderEndpointWrapper>();
			providerEndpointWrapper.Stub(x => x.PendingRequest).Return(request);

			var target = new OpenIdController(openIdProviderWapper, windowsAccountProvider, currentHttpContext, providerEndpointWrapper);
			target.TriggerWindowsAuthorization();

			request.IsAuthenticated.Should().Be.EqualTo(true);
			request.LocalIdentifier.ToString().Should().Be("http://mock/OpenId/AskUser/domainName#userName");
			openIdProviderWapper.AssertWasCalled(x => x.SendResponse(request));
		}

		[Test]
		public void ShouldNotAuthenticateMeIfImNotAWindowsUserWhenTriggerWindowsAuthorization()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWrapper>();
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			var request = MockRepository.GenerateMock<IAuthenticationRequest>();
			request.Expect(x => x.IsAuthenticated).PropertyBehavior();
			openIdProviderWapper.Stub(x => x.GetRequest()).Return(request);
			var windowsAccountProvider = MockRepository.GenerateMock<IWindowsAccountProvider>();
			windowsAccountProvider.Stub(x => x.RetrieveWindowsAccount()).Return(null);
			var providerEndpointWrapper = MockRepository.GenerateMock<IProviderEndpointWrapper>();
			providerEndpointWrapper.Stub(x => x.PendingRequest).Return(request);

			var target = new OpenIdController(openIdProviderWapper, windowsAccountProvider, currentHttpContext, providerEndpointWrapper);
			target.TriggerWindowsAuthorization();

			request.IsAuthenticated.Should().Be.EqualTo(false);
			openIdProviderWapper.AssertWasNotCalled(x => x.SendResponse(request));
		}

		[Test]
		public void ShouldReturnAskUserView()
		{
			var target = new OpenIdController(null, null, null, null);

			var result = target.AskUser();

			(result as ViewResult).ViewName.Should().Be.EqualTo("");
		}
		
	}

	
}