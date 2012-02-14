using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationControllerTest
	{
		private AuthenticationController _target;

		[SetUp]
		public void Setup()
		{
			_target = new AuthenticationController(null, null, null, null, null, null);
		}

		[TearDown]
		public void Teardown()
		{
			_target.Dispose();
		}

		[Test]
		public void DefaultActionShouldRedirectToSignIn()
		{
			var result = _target.Index() as RedirectToRouteResult;
			result.RouteValues["action"].Should().Be.EqualTo("SignIn");
		}
	}
}