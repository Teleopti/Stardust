using System;
using System.Net.Http;
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
		public void ShouldTriggerWindowsAuthorizationWhenResponseIsNotReady()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWrapper>();
			var request = MockRepository.GenerateMock<IAuthenticationRequest>();
			var providerEndpointWrapper = MockRepository.GenerateMock<IProviderEndpointWrapper>();
			var httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
			var httpContext = MockRepository.GenerateStub<HttpContextBase>();
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();

			request.Stub(x => x.IsResponseReady).Return(false);
			openIdProviderWapper.Stub(x => x.GetRequest()).Return(request);
			httpContext.Stub(x => x.Request).Return(httpRequest);
			currentHttpContext.Stub(x => x.Current()).Return(httpContext);
			httpRequest.Stub(x => x.HttpMethod).Return(HttpMethod.Get.Method);
			providerEndpointWrapper.Expect(x => x.PendingRequest).PropertyBehavior();

			var target = new OpenIdController(openIdProviderWapper, null, currentHttpContext, providerEndpointWrapper);
			var result = (ViewResult)target.Provider();

			providerEndpointWrapper.PendingRequest.Should().Be.SameInstanceAs(request);
			result.ViewName.Should().Be.EqualTo("SignIn");
		}

		[Test]
		public void ShouldHandleHeadRequest()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWrapper>();
			var request = MockRepository.GenerateMock<IAuthenticationRequest>();
			var providerEndpointWrapper = MockRepository.GenerateMock<IProviderEndpointWrapper>();
			var httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
			var httpContext = MockRepository.GenerateStub<HttpContextBase>();
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();

			request.Stub(x => x.IsResponseReady).Return(false);
			openIdProviderWapper.Stub(x => x.GetRequest()).Return(request);
			httpContext.Stub(x => x.Request).Return(httpRequest);
			currentHttpContext.Stub(x => x.Current()).Return(httpContext);
			httpRequest.Stub(x => x.HttpMethod).Return(HttpMethod.Head.Method);
			providerEndpointWrapper.Expect(x => x.PendingRequest).PropertyBehavior();

			var target = new OpenIdController(openIdProviderWapper, null, currentHttpContext, providerEndpointWrapper);
			var result = target.Provider();

			(result is EmptyResult).Should().Be.True();
		}

		[Test]
		public void ProviderShouldSimplyRespondIfTheResponseIsReady()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWrapper>();
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			
			var request = MockRepository.GenerateMock<IAuthenticationRequest>();
			var httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
			var httpContext = MockRepository.GenerateStub<HttpContextBase>();
			var outgoingWebResponse = MockRepository.GenerateMock<OutgoingWebResponse>();
			var windowsAccountProvider = MockRepository.GenerateMock<IWindowsAccountProvider>();
			var providerEndpointWrapper = MockRepository.GenerateMock<IProviderEndpointWrapper>();

			httpContext.Stub(x => x.Request).Return(httpRequest);
			currentHttpContext.Stub(x => x.Current()).Return(httpContext);
			httpRequest.Stub(x => x.HttpMethod).Return(HttpMethod.Get.Method);
			request.Stub(x => x.IsResponseReady).Return(true);
			openIdProviderWapper.Stub(x => x.GetRequest()).Return(request);
			openIdProviderWapper.Stub(x => x.PrepareResponse(request)).Return(outgoingWebResponse);
			
			var target = new OpenIdController(openIdProviderWapper, windowsAccountProvider, currentHttpContext, providerEndpointWrapper);
			target.Provider();

			openIdProviderWapper.AssertWasCalled(x => x.PrepareResponse(request));
			openIdProviderWapper.AssertWasNotCalled(x => x.SendResponse(request));
		}

		[Test]
		public void ShouldAuthenticateMeIfImAWindowsUserWhenTriggerWindowsAuthorization()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWrapper>();
			var request = MockRepository.GenerateMock<IAuthenticationRequest>();
			var httpResponse = MockRepository.GenerateStub<HttpResponseBase>();
			var httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			var httpContext = MockRepository.GenerateStub<HttpContextBase>();
			
			request.Expect(x => x.IsAuthenticated).PropertyBehavior();
			request.Expect(x => x.LocalIdentifier).PropertyBehavior();
			request.Expect(x => x.IsReturnUrlDiscoverable(null)).IgnoreArguments().Return(RelyingPartyDiscoveryResult.Success);
			openIdProviderWapper.Stub(x => x.GetRequest()).Return(request);

			var windowsAccountProvider = MockRepository.GenerateMock<IWindowsAccountProvider>();
			windowsAccountProvider.Stub(x => x.RetrieveWindowsAccount()).Return(new WindowsAccount("domainName", "user.Name"));

			httpResponse.Stub(x => x.ApplyAppPathModifier("~/OpenId/AskUser/domainName%23user%24%24%24Name")).Return("/OpenId/AskUser/domainName#user$$$Name");
			httpRequest.Stub(x => x.Url).Return(new Uri("http://mock"));
			httpContext.Stub(x => x.Response).Return(httpResponse);
			httpContext.Stub(x => x.Request).Return(httpRequest);
			currentHttpContext.Stub(x => x.Current()).Return(httpContext);

			var providerEndpointWrapper = MockRepository.GenerateMock<IProviderEndpointWrapper>();
			providerEndpointWrapper.Stub(x => x.PendingRequest).Return(request);

			var target = new OpenIdController(openIdProviderWapper, windowsAccountProvider, currentHttpContext, providerEndpointWrapper);
			target.TriggerWindowsAuthorization();

			request.IsAuthenticated.Should().Be.EqualTo(true);
			request.LocalIdentifier.ToString().Should().Be("http://mock/OpenId/AskUser/domainName#user$$$Name");
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

		[Test]
		public void ShouldNotAuthenticateIfNoServiceDocument()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWrapper>();
			var request = MockRepository.GenerateMock<IAuthenticationRequest>();
			request.Expect(x => x.IsAuthenticated).PropertyBehavior();
			request.Expect(x => x.LocalIdentifier).PropertyBehavior();
			request.Expect(x => x.IsReturnUrlDiscoverable(null)).IgnoreArguments().Return(RelyingPartyDiscoveryResult.NoServiceDocument);
			openIdProviderWapper.Stub(x => x.GetRequest()).Return(request);

			var windowsAccountProvider = MockRepository.GenerateMock<IWindowsAccountProvider>();
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			var providerEndpointWrapper = MockRepository.GenerateMock<IProviderEndpointWrapper>();
			providerEndpointWrapper.Stub(x => x.PendingRequest).Return(request);

			var target = new OpenIdController(openIdProviderWapper, windowsAccountProvider, currentHttpContext, providerEndpointWrapper);
			target.TriggerWindowsAuthorization();

			request.IsAuthenticated.Should().Be.EqualTo(false);
			
		}
	}

	
}