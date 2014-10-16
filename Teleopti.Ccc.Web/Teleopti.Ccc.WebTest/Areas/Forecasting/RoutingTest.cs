using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Forecasting;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting
{
	public class RoutingTest
	{
		[Test]
		public void ShouldRouteControllerAndActionInForecasting()
		{
			var routes = new RouteCollection();
			var target = new ForecastingAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/api/Forecasting/SomeController/SomeId");
			var routeData = routes.GetRouteData(httpContext);

			routeData.Values["controller"].Should().Be("SomeController");
			routeData.Values["id"].Should().Be("SomeId");
		}
	}
}