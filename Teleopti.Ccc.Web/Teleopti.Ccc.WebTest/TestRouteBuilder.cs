using System.Web.Mvc;
using System.Web.Routing;

namespace Teleopti.Ccc.WebTest
{
	public class TestRouteBuilder
	{
		public RouteCollection MakeDefaultRoute()
		{
			var routes = new RouteCollection();
			routes.MapRoute(
				"route-default",
				"{controller}/{action}/{id}"
				);

			return routes;
		}

		public RouteCollection MakeAreaDefaultRoute(string area)
		{
			var routes = new RouteCollection();
			routes.MapRoute(
				"area-default",
				area + "/{controller}/{action}/{id}"
				);

			return routes;
		}

		public RouteCollection MakeAreaWithDefaults(string areaName)
		{
			var routes = new RouteCollection();

			var mapRoute = routes.MapRoute("area-origin",
			                                 areaName + "/{controller}/{action}/{id}", new
			                                                                           	{
			                                                                           		area = areaName,
			                                                                           		controller = "Controller",
			                                                                           		action = "Index",
			                                                                           		id =
			                                                                           	UrlParameter.Optional
			                                                                           	});
			mapRoute.DataTokens["area"] = areaName;
			routes.MapRoute(
				"route-default",
				"{controller}/{action}/{id}", new {controller = "Home", action = "Index", id = UrlParameter.Optional}
				);
			return routes;
		}
	}
}