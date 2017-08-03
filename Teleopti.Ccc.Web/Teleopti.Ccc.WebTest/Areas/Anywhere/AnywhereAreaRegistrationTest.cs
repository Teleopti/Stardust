using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Anywhere;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class AnywhereAreaRegistrationTest
	{
		[Test]
		public void ShouldRedirectAnywhereAreaToWfm()
		{
			var routes = new RouteCollection();
			var target = new AnywhereAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/Anywhere/Controller/Action");

			var routeData = routes.GetRouteData(httpContext);
			routeData.Values["controller"].Should().Be("Controller");
			routeData.Values["action"].Should().Be("Action");

			// Anywhere is not supported any more, now access to Anywhere will be redirect to WFM
			// Refer to bug #45319: Users able to access old Anywhere in Web using bookmarked link
			var defaultRoute = ((Route)routeData.Route).Defaults;
			defaultRoute["controller"].Should().Be("Application");
			defaultRoute["action"].Should().Be("Redirect");
		}

		[Test]
		public void ShouldRouteAnywhereAuthenticationToStartArea()
		{
			var routes = new RouteCollection();
			var target = new AnywhereAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/Anywhere/Authentication/SignIn");
			var routeData = routes.GetRouteData(httpContext);

			var namespaces = routeData.DataTokens["namespaces"] as IEnumerable<string>;
			namespaces.Should().Have.SameValuesAs("Teleopti.Ccc.Web.Areas.Start.*");
			routeData.Values["origin"].Should().Be("Anywhere");
			routeData.DataTokens["area"].Should().Be("Start");
		}
	}
}