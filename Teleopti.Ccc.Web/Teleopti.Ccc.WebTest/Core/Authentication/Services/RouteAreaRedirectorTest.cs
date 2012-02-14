using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.WebTest.Core.Authentication.Services
{
	[TestFixture]
	public class RouteAreaRedirectorTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			var urlHelperBuilder = new TestUrlHelperBuilder();
			urlHelperBuilder.Routes(new TestRouteBuilder().MakeAreaWithDefaults("Area"));
			_urlHelper = urlHelperBuilder.MakeUrlHelper("http://hostname/Area/Controller/Action", new { area = "Area", controller = "controller", action = "action" });
		}

		#endregion

		private UrlHelper _urlHelper;

		[Test]
		public void SignInShouldRedirectToOriginIfPresent()
		{
			var target = new Redirector(_urlHelper);
			_urlHelper.RequestContext.RouteData.Values["origin"] = "Area";

			var signInRedirect
				= target.SignInRedirect();

			signInRedirect.Url.Should().Be.EqualTo("/Area");
		}

		[Test]
		public void SignInShouldRedirectToRoot()
		{
			var target = new Redirector(_urlHelper);

			var signInRedirect
				= target.SignInRedirect();

			signInRedirect.Url.Should().Be.EqualTo("/");
		}

		[Test]
		public void SignOutShouldRedirectToRoot()
		{

			var target = new Redirector(_urlHelper);

			var signOutRedirect = target.SignOutRedirect(string.Empty);

			signOutRedirect.Url.Should().Be.EqualTo("/");
		}
	}
}