using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationControllerSignOutTest
	{
		private AuthenticationController _target;
		private MockRepository mocks;
		private IFormsAuthentication _formsAuthentication;
		private IRedirector _redirector;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			_formsAuthentication = mocks.DynamicMock<IFormsAuthentication>();
			_redirector = mocks.DynamicMock<IRedirector>();
			_target = new AuthenticationController(null, null, null, _formsAuthentication, null, _redirector);
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
			_redirector.Stub(x => x.SignOutRedirect(string.Empty)).Return(new RedirectResult("/"));
			using (mocks.Record())
			{
				Expect.Call(_formsAuthentication.SignOut);
			}
			using (mocks.Playback())
			{
				var result = _target.SignOut(string.Empty) as RedirectResult;

				result.Url.Should().Be.EqualTo("/");
				/*
				result.RouteValues["controller"].Should().Equals("home");
				result.RouteValues["action"].Should().Equals("index");
				 * */
			}
		}

		[Test]
		public void ShouldRedirectToUrl()
		{
			_redirector.Stub(x => x.SignOutRedirect(string.Empty)).Return(new RedirectResult("/"));
			using (mocks.Record())
			{
				Expect.Call(_formsAuthentication.SignOut);
				Expect.Call(_redirector.SignOutRedirect("/a/url")).Return(new RedirectResult("/a/url"));
			}
			using (mocks.Playback())
			{
				var result = _target.SignOut("/a/url") as RedirectResult;

				result.Url.Should().Be.EqualTo("/a/url");
			}
		}

		[Test]
		[Ignore]
		public void ShouldRedirectToHomeControllerWhenNonLocalUrl()
		{
			Assert.Fail("Not implemented.");
		}
	}
}