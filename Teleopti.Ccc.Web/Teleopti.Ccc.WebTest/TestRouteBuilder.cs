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
	}
}