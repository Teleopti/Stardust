using System;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
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
		private MockRepository mocks;
		private IFormsAuthentication _formsAuthentication;
		private ISessionSpecificWfmCookieProvider _sessionSpecificWfmCookieProvider;
		private IAuthenticationModule _authenticationModule;


		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			_formsAuthentication = mocks.DynamicMock<IFormsAuthentication>();
			_sessionSpecificWfmCookieProvider = mocks.DynamicMock<ISessionSpecificWfmCookieProvider>();
			_authenticationModule = MockRepository.GenerateMock<IAuthenticationModule>();
			_authenticationModule.Stub(x => x.Issuer(null)).IgnoreArguments().Return(new Uri("http://issuer"));
			_authenticationModule.Stub(x => x.Realm).Return("testrealm");
		}

		[Test]
		public void ShouldSignOut()
		{
			var target = new AuthenticationController(null, _formsAuthentication, _sessionSpecificWfmCookieProvider, _authenticationModule,  new FakeCurrentHttpContext(new FakeHttpContext()), null, null, null, new FakeCurrentTeleoptiPrincipal());
			new TestControllerBuilder().InitializeController(target);
			
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/TeleoptiCCC/web/"), new Uri("http://localhost/TeleoptiCCC/web/"));
			request.Stub(x => x.Url).Return(new Uri("http://localhost/TeleoptiCCC/web/Authentication/SignOut"));
			var context = new FakeHttpContext("/");
			context.SetRequest(request);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			using (mocks.Record())
			{
				Expect.Call(_formsAuthentication.SignOut);
				Expect.Call(_sessionSpecificWfmCookieProvider.RemoveCookie);
			}
			using (mocks.Playback())
			{
				var result = target.SignOut() as RedirectResult;

				result.Url.Should()
					.Be.EqualTo("http://issuer/?wa=wsignout1.0&wreply=http%3a%2f%2fissuer%2f%3fwa%3dwsignin1.0%26wtrealm%3dtestrealm%26wctx%3dru%253d%252fTeleoptiCCC%252fweb%252f");
			}
		}

		[Test]
		public void ShouldInvalidateCachedCulture()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var personPersister = MockRepository.GenerateMock<IPersonPersister>();
			var principal = new TeleoptiPrincipalForLegacy(new TeleoptiIdentity("name", null, null, null, null), person);
			var currentTeleoptiPrincipal = new FakeCurrentTeleoptiPrincipal(principal);
			
			var target = new AuthenticationController(null, _formsAuthentication, _sessionSpecificWfmCookieProvider, _authenticationModule,  new FakeCurrentHttpContext(new FakeHttpContext()), null, loggedOnUser, personPersister, currentTeleoptiPrincipal);
			new TestControllerBuilder().InitializeController(target);
			
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/TeleoptiCCC/web/"), new Uri("http://localhost/TeleoptiCCC/web/"));
			request.Stub(x => x.Url).Return(new Uri("http://localhost/TeleoptiCCC/web/Authentication/SignOut"));
			var context = new FakeHttpContext("/");
			context.SetRequest(request);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			target.SignOut();
			
			personPersister.AssertWasCalled(x=>x.InvalidateCachedCulture(person));
		}
	}
}