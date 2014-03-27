using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationControllerSignOutTest
	{
		private AuthenticationController _target;
		private MockRepository mocks;
		private IFormsAuthentication _formsAuthentication;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			_formsAuthentication = mocks.DynamicMock<IFormsAuthentication>();
			_target = new AuthenticationController(null, _formsAuthentication);
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
	}
}