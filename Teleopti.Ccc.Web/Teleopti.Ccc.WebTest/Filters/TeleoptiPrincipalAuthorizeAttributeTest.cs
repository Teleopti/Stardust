using System;
using System.Threading;
using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class TeleoptiPrincipalAuthorizeAttributeTest
	{
		[Test]
		public void ShouldReturnUnauthorizedResultWhenGenericPrincipal()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute();
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<HttpUnauthorizedResult>();
		}

		[Test, Ignore("Should work without other handling")]
		public void ShouldRedirectToStartAuthenticationSignIn()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute();
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target) as RedirectToRouteResult;
			result.RouteValues.Values.Should().Have.SameValuesAs("Start", "Authentication", "");
		}

		[Test, Ignore("Might not be needed any longer")]
		public void ShouldGetDefaultHomeRealm()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute();
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target) as RedirectToRouteResult;
			result.RouteValues.Values.Should().Have.SameValuesAs("Return", "Hash", "Start", "http://myissuer/?wa=wsignin1.0&wtrealm=http%3a%2f%2fmytime&wctx=ru%3d&whr=urn%3aProviderX");
		}

		[Test, Ignore("Might not be needed any longer")]
		public void ShouldRedirectToAreasAuthenticationSignIn()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute();
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);
			filterTester.AddRouteDataToken("area", "MyTime");

			var result = filterTester.InvokeFilter(target) as RedirectToRouteResult;
			result.RouteValues["redirectUrl"].ToString().Should().Contain("");
		}

		[Test]
		public void ShouldReturnActionsResultWhenAuthenticated()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute();
			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => new ViewResult());
			filterTester.IsUser(new TeleoptiPrincipal(new TeleoptiIdentity("_", null, null, null, null), null));

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<ViewResult>();
		}

		[Test]
		public void ShouldReturnViewResultWithGenericPrincipalWhenExcluded()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(new[] {typeof (FilterTester.TestController)});
			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => new ViewResult());

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<ViewResult>();
		}

		[Test]
		public void ShouldBeAbleAccessControllersExcludedByBaseType()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(new[] { typeof(FilterTester.TestController) });
			var filterTester = new FilterTester();
			filterTester.UseController(new TestControllerProxy(() => new ViewResult()));

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<ViewResult>();
		}

		[Test]
		public void ShouldReturnHttp403ForbiddenResultWhenAjax()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute();
			var filterTester = new FilterTester();
			filterTester.IsAjaxRequest();

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<HttpUnauthorizedResult>();
		}

		public class TestControllerProxy : FilterTester.TestController
		{
			public TestControllerProxy(Func<ActionResult> controllerAction) : base(controllerAction)
			{
			}
		}
	}
}