using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Messages;
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
			httpRequest.Stub(x => x.HttpMethod).Return("GET");
			providerEndpointWrapper.Expect(x => x.PendingRequest).PropertyBehavior();

			var target = new OpenIdController(openIdProviderWapper, null, currentHttpContext, providerEndpointWrapper);
			var result = (RedirectToRouteResult)target.Provider();

			providerEndpointWrapper.PendingRequest.Should().Be.SameInstanceAs(request);
			result.RouteValues.ContainsValue("TriggerWindowsAuthorization").Should().Be.True();
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
			httpRequest.Stub(x => x.HttpMethod).Return("HEAD");
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
			httpRequest.Stub(x => x.HttpMethod).Return("GET");
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

		[Test]
		public void ShouldSetIsPersistentToTrue()
		{
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWrapper>();
			var request = new FakeAuthenticationRequest();
			var httpResponse = MockRepository.GenerateStub<HttpResponseBase>();
			var httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			var httpContext = MockRepository.GenerateStub<HttpContextBase>();

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

			var fetchResponse = request.GetExtension<FetchResponse>();
			fetchResponse.GetAttributeValue(ClaimTypes.IsPersistent).Should().Be.EqualTo("true");
		}
	}

	public class FakeAuthenticationRequest : IAuthenticationRequest
	{
		private readonly IList<IOpenIdMessageExtension> extensionList = new List<IOpenIdMessageExtension>();

		public void AddResponseExtension(IOpenIdMessageExtension extension)
		{
			extensionList.Add(extension);
		}

		public void ClearResponseExtensions()
		{
			throw new NotImplementedException();
		}

		public T GetExtension<T>() where T : IOpenIdMessageExtension, new()
		{
			return (T) extensionList.First();
		}

		public IOpenIdMessageExtension GetExtension(Type extensionType)
		{
			throw new NotImplementedException();
		}

		public bool IsResponseReady { get; private set; }
		public ProviderSecuritySettings SecuritySettings { get; set; }
		public RelyingPartyDiscoveryResult IsReturnUrlDiscoverable(IDirectWebRequestHandler webRequestHandler)
		{
			return RelyingPartyDiscoveryResult.Success;
		}

		public ProtocolVersion RelyingPartyVersion { get; private set; }
		public Realm Realm { get; private set; }
		public bool Immediate { get; private set; }
		public Uri ProviderEndpoint { get; set; }
		public void SetClaimedIdentifierFragment(string fragment)
		{
			throw new NotImplementedException();
		}

		public bool IsDirectedIdentity { get; private set; }
		public bool IsDelegatedIdentifier { get; private set; }
		public Identifier LocalIdentifier { get; set; }
		public Identifier ClaimedIdentifier { get; set; }
		public bool? IsAuthenticated { get; set; }
	}
}