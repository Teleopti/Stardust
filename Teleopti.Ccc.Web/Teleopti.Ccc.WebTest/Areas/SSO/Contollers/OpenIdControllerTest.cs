using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Messages;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System.Web.Routing;
using DotNetOpenAuth.OpenId.Provider;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.SSO.Controllers;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Core;

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
			var target = new OpenIdController(null, null, null, null, getToggleManager());
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			var result = target.Identifier();

			(result as ViewResult).ViewName.Should().Be.EqualTo("Xrds");
		}

		private static FakeToggleManager getToggleManager()
		{
			var toggleManager = new FakeToggleManager();
			return toggleManager;
		}

		[Test]
		public void IdentifierShouldReturnView()
		{
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://mock/"), new Uri("http://mock/"));
			var context = new FakeHttpContext("/");
			context.SetRequest(request);
			var target = new OpenIdController(null, null, null, null, getToggleManager());
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			var result = target.Identifier();

			(result as ViewResult).ViewName.Should().Be.EqualTo("");
		}

		[Test]
		public void ShouldReturnAskUserView()
		{
			var target = new OpenIdController(null, null, null, null, getToggleManager());

			var result = target.AskUser();

			(result as ViewResult).ViewName.Should().Be.EqualTo("");
		}

		[Test]
		public void ShouldSetIsPersistentToTrue()
		{
			var formsAuthentication = new FakeFormsAuthentication();
			var httpContextBase = new FakeHttpContext("testRelative/");
			httpContextBase.SetRequest(new FakeHttpRequest("testRelative/", new Uri("http://test/"), new Uri("http://referer/")));
			var openIdProviderWapper = MockRepository.GenerateMock<IOpenIdProviderWapper>();
			var target = new OpenIdController(openIdProviderWapper, new FakeCurrentHttpContext(httpContextBase), null, formsAuthentication, getToggleManager());

			var request = new FakeAuthenticationRequest
			{
				IsDirectedIdentity = true
			};

			var outgoingWebResponse = new OutgoingWebResponse();
			openIdProviderWapper.Stub(
				x =>
					x.PrepareResponse(
						Arg<IHostProcessedRequest>.Matches(
							r => r.GetExtension<FetchResponse>().GetAttributeValue(ClaimTypes.IsPersistent) == "true")))
				.Return(outgoingWebResponse);

			target.ProcessAuthRequest(Convert.ToBase64String(SerializationHelper.SerializeAsBinary(request).ToCompressedByteArray()), true);
			openIdProviderWapper.AssertWasCalled(
				x =>
					x.PrepareResponse(
						Arg<IHostProcessedRequest>.Matches(
							r => r.GetExtension<FetchResponse>().GetAttributeValue(ClaimTypes.IsPersistent) == "true")));
		}

	}

	public class FakeFormsAuthentication : IFormsAuthentication
	{
		public void SetAuthCookie(string userName, bool isPersistent, bool isLogonFromBrowser, string tenantName)
		{
			throw new NotImplementedException();
		}

		public void SignOut()
		{
			throw new NotImplementedException();
		}

		public bool TryGetCurrentUser(out string userName)
		{
			userName = "testUser";
			return true;
		}
	}

	[Serializable]
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
			return (T)extensionList.First();
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

		public bool IsDirectedIdentity { get; set; }
		public bool IsDelegatedIdentifier { get; private set; }
		public Identifier LocalIdentifier { get; set; }
		public Identifier ClaimedIdentifier { get; set; }
		public bool? IsAuthenticated { get; set; }
	}
}