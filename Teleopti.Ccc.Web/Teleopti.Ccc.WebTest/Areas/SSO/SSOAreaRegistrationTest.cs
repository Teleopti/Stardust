using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.SSO;

namespace Teleopti.Ccc.WebTest.Areas.SSO
{
	[TestFixture]
	public class SSOAreaRegistrationTest
	{

		[Test]
		public void ShouldRouteSSOArea()
		{
			var routes = new RouteCollection();
			var target = new SSOAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/SSO/Controller/Action");
			var routeData = routes.GetRouteData(httpContext);

			routeData.Values["controller"].Should().Be("Controller");
			routeData.Values["action"].Should().Be("Action");
		}
	}
}