using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.Anywhere;
using Teleopti.Ccc.Web.Areas.MyTime;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class AnywhereAreaRegistrationTest
	{

		[Test]
		public void ShouldRouteAnywhereArea()
		{
			var routes = new RouteCollection();
			var target = new AnywhereAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/Anywhere/Controller/Action");
			var routeData = routes.GetRouteData(httpContext);

			var expectedValues = new Dictionary<string, string> { { "controller", "Controller" }, { "action", "Action" } };
			expectedValues.ForEach(x => routeData.Values[x.Key].Should().Be.EqualTo(x.Value));
		}

		[Test]
		public void ShouldRouteMyTimeAuthenticationToStartArea()
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
			routeData.DataTokens["area"].Should().Be("Start");
		}
	}
}