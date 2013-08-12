using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.PerformanceTool;

namespace Teleopti.Ccc.WebTest.Areas.PerformanceTool
{
	[TestFixture]
	public class PerformanceToolAreaRegistrationTest
	{

		[Test]
		public void ShouldRoutePerformanceToolArea()
		{
			var routes = new RouteCollection();
			var target = new PerformanceToolAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/PerformanceTool/Controller/Action");
			var routeData = routes.GetRouteData(httpContext);

			routeData.Values["controller"].Should().Be("Controller");
			routeData.Values["action"].Should().Be("Action");
		}

		[Test]
		public void ShouldRoutePerformanceToolAuthenticationToStartArea()
		{
			var routes = new RouteCollection();
			var target = new PerformanceToolAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/PerformanceTool/Authentication/SignIn");
			var routeData = routes.GetRouteData(httpContext);

			var namespaces = routeData.DataTokens["namespaces"] as IEnumerable<string>;
			namespaces.Should().Have.SameValuesAs("Teleopti.Ccc.Web.Areas.Start.*");
			routeData.Values["origin"].Should().Be("PerformanceTool");
			routeData.DataTokens["area"].Should().Be("Start");
		}
	}
}