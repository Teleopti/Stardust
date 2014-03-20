using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.MyTime;
using Teleopti.Ccc.Web.Areas.Start;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class RegisterRoutesTaskTest
	{
		private RegisterRoutesTask target;

		[SetUp]
		public void Setup()
		{
			target = new RegisterRoutesTask(r => r.Clear(), null);
		}

		[Test]
		public void StupidTestForNow()
		{
			//replace with good test if logic changes
			target.Execute();
		}
	}

	[TestFixture]
	public class RegisterRoutesTest
	{
		[Test]
		public void ShouldRouteDefaultToMenuIndex()
		{
			var routes = new RouteCollection();
			new RegisterRoutesTask(r => r.Clear(), null).registerRoutes(routes);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/");
			var routeData = routes.GetRouteData(httpContext);

			var expectedValues = new Dictionary<string, string> { { "controller", "Authentication" }, { "action", "Index" } };
			ExpectedValuesShouldExist(expectedValues, routeData);
		}

		[Test]
		public void ShouldRouteMyTimeArea()
		{
			var routes = new RouteCollection();
			var target = new MyTimeAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/MyTime/Tab/Sub");
			var routeData = routes.GetRouteData(httpContext);

			var expectedValues = new Dictionary<string, string> { { "controller", "Tab" }, { "action", "Sub" } };
			ExpectedValuesShouldExist(expectedValues, routeData);
		}

		[Test]
		public void ShouldRouteMyTimeAuthenticationToStartArea()
		{
			var routes = new RouteCollection();
			var target = new MyTimeAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/MyTime/Authentication/SignIn");
			var routeData = routes.GetRouteData(httpContext);

			var namespaces = routeData.DataTokens["namespaces"] as IEnumerable<string>;
			namespaces.Should().Have.SameValuesAs("Teleopti.Ccc.Web.Areas.Start.*");
			routeData.DataTokens["area"].Should().Be("Start");
		}

		[Test]
		public void ShouldRouteStartArea()
		{
			var routes = new RouteCollection();
			var target = new StartAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/Start/Tab/Sub");
			var routeData = routes.GetRouteData(httpContext);

			var expectedValues = new Dictionary<string, string> { { "controller", "Tab" }, { "action", "Sub" } };
			ExpectedValuesShouldExist(expectedValues, routeData);
		}

		[Test]
		public void ShouldRouteMyTimeDateParts()
		{
			var routes = new RouteCollection();
			var target = new MyTimeAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/MyTime/Tab/Sub/2011/05/31");
			var routeData = routes.GetRouteData(httpContext);

			var expectedValues = new Dictionary<string, string> { { "controller", "Tab" }, { "action", "Sub" }, { "year", "2011" }, { "month", "05" }, { "day", "31" } };
			ExpectedValuesShouldExist(expectedValues, routeData);
		}

		[Test]
		public void ShouldRouteMyTimeGuid()
		{
			var routes = new RouteCollection();
			var target = new MyTimeAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/MyTime/Tab/Sub/2e2f44e4-ae03-4990-94ce-1fe9933d52c5");
			var routeData = routes.GetRouteData(httpContext);

			var expectedValues = new Dictionary<string, string> { { "controller", "Tab" }, { "action", "Sub" }, { "id", "2e2f44e4-ae03-4990-94ce-1fe9933d52c5" } };
			ExpectedValuesShouldExist(expectedValues, routeData);
		}

		[Test]
		public void ShouldRouteMyTimeDatePartsAndGuid()
		{
			var routes = new RouteCollection();
			var target = new MyTimeAreaRegistration();
			var areaRegistrationContext = new AreaRegistrationContext(target.AreaName, routes);

			target.RegisterArea(areaRegistrationContext);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/MyTime/Tab/Sub/2011/05/31/2e2f44e4-ae03-4990-94ce-1fe9933d52c5");
			var routeData = routes.GetRouteData(httpContext);

			var expectedValues = new Dictionary<string, string> { { "controller", "Tab" }, { "action", "Sub" }, { "year", "2011" }, { "month", "05" }, { "day", "31" }, { "id", "2e2f44e4-ae03-4990-94ce-1fe9933d52c5" } };
			ExpectedValuesShouldExist(expectedValues, routeData);
		}

		[Test]
		public void ShouldIgnoreFilesInContentFolder()
		{
			var routes = new RouteCollection();
			new RegisterRoutesTask(r => r.Clear(), null).registerRoutes(routes);

			var httpContext = MockRepository.GenerateStub<HttpContextBase>();
			httpContext.Expect(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/Content/Script/AnyFile");
			var routeData = routes.GetRouteData(httpContext);

			routeData.RouteHandler.Should().Be.InstanceOf<StopRoutingHandler>();
		}

		[Test]
		public void ShouldMapAuthenticationRequestToStartArea()
		{
			var routes = new RouteCollection();
			new RegisterRoutesTask(r => r.Clear(), null).registerRoutes(routes);

			var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(c => c.Request.AppRelativeCurrentExecutionFilePath).Return("~/Authentication");
			var routeData = routes.GetRouteData(httpContext);

			var expectedValues = new Dictionary<string, string> { { "controller", "Authentication" }, { "action", "SignIn" }, { "area", "Start" }};
			ExpectedValuesShouldExist(expectedValues, routeData);
		}

		private void ExpectedValuesShouldExist(Dictionary<string, string> expectedValues, RouteData routeData)
		{
			expectedValues.ForEach(x => routeData.Values[x.Key].Should().Be.EqualTo(x.Value));
		}
	}

}