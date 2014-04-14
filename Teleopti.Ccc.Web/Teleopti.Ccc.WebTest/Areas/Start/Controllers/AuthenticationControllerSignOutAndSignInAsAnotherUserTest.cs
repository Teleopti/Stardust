using System;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationControllerSignOutAndSignInAsAnotherUserTest
	{
		private AuthenticationController _target;
		private MockRepository mocks;
		private IFormsAuthentication _formsAuthentication;
		private ISessionSpecificDataProvider _sessionSpecificDataProvider;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			_formsAuthentication = mocks.DynamicMock<IFormsAuthentication>();
			_sessionSpecificDataProvider = mocks.DynamicMock<ISessionSpecificDataProvider>();
			var authenticationModule = MockRepository.GenerateMock<IAuthenticationModule>();
			authenticationModule.Stub(x => x.Issuer).Return("http://issuer");
			authenticationModule.Stub(x => x.Realm).Return("testrealm");

			_target = new AuthenticationController(null, _formsAuthentication, _sessionSpecificDataProvider, authenticationModule);
			new MvcContrib.TestHelper.TestControllerBuilder().InitializeController(_target);
		}

		[TearDown]
		public void Teardown()
		{
			_target.Dispose();
		}

		[Test]
		public void ShouldRedirectToRootWhenNoReturnUrl()
		{
			using (mocks.Record())
			{
				Expect.Call(_formsAuthentication.SignOut);
			}
			using (mocks.Playback())
			{
				var result = _target.SignOut() as RedirectToRouteResult;

				result.RouteValues["action"].Should().Be.EqualTo("");
				result.RouteValues["controller"].Should().Be.EqualTo("Authentication");
			}
		}

		[Test]
		public void ShouldSignInAsAnotherUser()
		{
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/TeleoptiCCC/web/"), new Uri("http://localhost/TeleoptiCCC/web/"));
			request.Stub(x => x.Url).Return(new Uri("http://localhost/TeleoptiCCC/web/Authentication/SignInAsAnotherUser"));
			var context = new FakeHttpContext("/");
			context.SetRequest(request);
			_target.ControllerContext = new ControllerContext(context, new RouteData(), _target);

			using (mocks.Record())
			{
				Expect.Call(_formsAuthentication.SignOut);
				Expect.Call(_sessionSpecificDataProvider.RemoveCookie);
			}
			using (mocks.Playback())
			{
				var result = _target.SignInAsAnotherUser() as RedirectResult;

				result.Url.Should()
					.Be.EqualTo("http://issuer/?wa=wsignout1.0&wreply=http%3a%2f%2fissuer%2f%3fwa%3dwsignin1.0%26wtrealm%3dtestrealm%26wctx%3dru%253dhttp%253a%252f%252flocalhost%252fTeleoptiCCC%252fweb%252f");
			}
		}
	}
}