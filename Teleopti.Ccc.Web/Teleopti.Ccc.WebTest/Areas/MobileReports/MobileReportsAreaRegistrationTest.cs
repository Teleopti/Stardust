using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.MobileReports;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports
{
	[TestFixture]
	public class MobileReportsAreaRegistrationTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_target = new MobileReportsAreaRegistration();
		}

		#endregion

		private MobileReportsAreaRegistration _target;

		[Test]
		public void ShouldRegisterMobileReportsAuthetcationRoute()
		{
			const string routeName = "MobileReports-authentication";
			var routes = new RouteCollection();
			var areaRegistrationContext = new AreaRegistrationContext(_target.AreaName, routes);

			_target.RegisterArea(areaRegistrationContext);

			routes[routeName].Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterMobileReportsAuthetcationSignInRoute()
		{
			const string routeName = "MobileReports-authentication-signin";
			var routes = new RouteCollection();
			var areaRegistrationContext = new AreaRegistrationContext(_target.AreaName, routes);

			_target.RegisterArea(areaRegistrationContext);

			routes[routeName].Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRegisterMobileReportsWithDefaultName()
		{
			const string defaultRouteName = "MobileReports-default";
			var routes = new RouteCollection();
			var areaRegistrationContext = new AreaRegistrationContext(_target.AreaName, routes);

			_target.RegisterArea(areaRegistrationContext);

			routes[defaultRouteName].Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRouteMobileReportsAuthenticationRoute()
		{
			var routes = new RouteCollection();
			var areaRegistrationContext = new AreaRegistrationContext(_target.AreaName, routes);

			_target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/MobileReports/Authentication");
			var routeData = routes.GetRouteData(httpContext);

			var namespaces = routeData.DataTokens["namespaces"] as IEnumerable<string>;
			namespaces.Should().Have.SameValuesAs("Teleopti.Ccc.Web.Areas.Start.*");
			routeData.DataTokens["area"].Should().Be("Start");
		}

		[Test]
		public void ShouldRouteMobileReportsAuthenticationSignInToMobileSignin()
		{
			var routes = new RouteCollection();
			var areaRegistrationContext = new AreaRegistrationContext(_target.AreaName, routes);

			_target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/MobileReports/Authentication/SignIn");
			var routeData = routes.GetRouteData(httpContext);

			var expectedValues = new Dictionary<string, string>
			                     	{{"area", "Start"}, {"controller", "Authentication"}, {"action", "MobileSignIn"}};
			expectedValues.ForEach(x => routeData.Values[x.Key].Should().Be.EqualTo(x.Value));
		}

		[Test]
		public void ShouldRouteMobileReportsDefaultRoute()
		{
			var routes = new RouteCollection();
			var areaRegistrationContext = new AreaRegistrationContext(_target.AreaName, routes);

			_target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/MobileReports");

			var routeData = routes.GetRouteData(httpContext);

			routeData.DataTokens["area"].Should().Be("MobileReports");
		}
	}
}