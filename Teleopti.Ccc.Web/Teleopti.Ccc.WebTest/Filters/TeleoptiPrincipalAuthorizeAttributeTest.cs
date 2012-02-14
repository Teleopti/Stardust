using System.Security.Principal;
using System.Threading;
using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
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
			result.RouteValues.Values.Should().Have.SameValuesAs("Start", "Authentication", "SignIn");
		}

		[Test]
		public void ShouldRedirectToAreasAuthenticationSignIn()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute();
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);
			filterTester.AddRouteDataToken("area", "MyTime");

			var result = filterTester.InvokeFilter(target) as RedirectToRouteResult;
			result.RouteValues.Values.Should().Have.SameValuesAs("MyTime", "Authentication", "SignIn");
		}

		[Test]
		public void ShouldReturnActionsResultWhenTeleoptiPrincipal()
		{
			var principal = new TeleoptiPrincipal(new GenericIdentity("me", "custom"), new Person());
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
		
	}
}