using System.Web.Mvc;
using System.Web.Routing;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.WebTest
{
	public class TestUrlHelperBuilder
	{
		private RouteCollection _routes = new TestRouteBuilder().MakeDefaultRoute();

		public void Routes(RouteCollection routes)
		{
			_routes = routes;
		}

		public UrlHelper MakeUrlHelper(string url)
		{
			var httpContext = new TestHttpContextBuilder().StubHttpContext(url);
			return new UrlHelper(new RequestContext(httpContext, new RouteData()), _routes);
		}
	}
}