using System;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationControllerSignOutTest
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
			authenticationModule.Stub(x => x.Issuer(null)).IgnoreArguments().Return(new Uri("http://issuer"));
			authenticationModule.Stub(x => x.Realm).Return("testrealm");

			_target = new AuthenticationController(null, _formsAuthentication, _sessionSpecificDataProvider, authenticationModule,  new FakeCurrentHttpContext(new FakeHttpContext()), null);
			new TestControllerBuilder().InitializeController(_target);
		}

		[TearDown]
		public void Teardown()
		{
			_target.Dispose();
		}

		[Test]
		public void ShouldSignOut()
		{
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/TeleoptiCCC/web/"), new Uri("http://localhost/TeleoptiCCC/web/"));
			request.Stub(x => x.Url).Return(new Uri("http://localhost/TeleoptiCCC/web/Authentication/SignOut"));
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
				var result = _target.SignOut() as RedirectResult;

				result.Url.Should()
					.Be.EqualTo("http://issuer/?wa=wsignout1.0&wreply=http%3a%2f%2fissuer%2f%3fwa%3dwsignin1.0%26wtrealm%3dtestrealm%26wctx%3dru%253d%252fTeleoptiCCC%252fweb%252f");
			}
		}
	}
}