using System.Threading;
using System.Web.Mvc;
using Microsoft.IdentityModel.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class TeleoptiPrincipalAuthorizeAttributeTest
	{
		[Test]
		public void ShouldReturnRedirectResultWhenGenericPrincipal()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute();
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<RedirectToRouteResult>();
		}

		[Test]
		public void ShouldRedirectToStartAuthenticationSignIn()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute();
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target) as RedirectToRouteResult;
			result.RouteValues.Values.Should().Have.SameValuesAs("Start", "Authentication", "");
		}

		[Test]
		public void ShouldRedirectToAreasAuthenticationSignIn()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute();
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);
			filterTester.AddRouteDataToken("area", "MyTime");

			var result = filterTester.InvokeFilter(target) as RedirectToRouteResult;
			result.RouteValues.Values.Should().Have.SameValuesAs("MyTime", "Authentication", "");
		}

		[Test]
		public void ShouldReturnActionsResultWhenTeleoptiPrincipal()
		{
			var principal = MockRepository.GenerateMock<ITeleoptiPrincipal>();
			var target = new TeleoptiPrincipalAuthorizeAttribute();
			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => new ViewResult());
			filterTester.IsUser(principal);

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<ViewResult>();
		}

		[Test]
		public void ShouldReturnViewResultWithGenericPrincipalWhenExcluded()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(new[] { typeof(FilterTester.TestController) });
			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => new ViewResult());
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<ViewResult>();
		}

		[Test]
		public void ShouldReturnHttp403ForbiddenResultWhenAjax()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute();
			var filterTester = new FilterTester();
			filterTester.IsAjaxRequest();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target) as HttpStatusCodeResult;

			result.StatusCode.Should().Be(403); 
			// 403: forbidden.
			// 401 has strange behavior on iis7/ie/intranet (or something) and will display a dialog even on ajax requests!
		}
		
	}
}