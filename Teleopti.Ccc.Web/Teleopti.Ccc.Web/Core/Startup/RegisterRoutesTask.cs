using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Owin;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(2)]
	public class RegisterRoutesTask : IBootstrapperTask
	{
		private readonly IRegisterAreas _registerAreas;
		private readonly IGlobalConfiguration _globalConfiguration;
		private readonly Action<RouteCollection> _runBefore;

		public RegisterRoutesTask(IRegisterAreas registerAreas, IGlobalConfiguration globalConfiguration)
			: this(r => { }, registerAreas, globalConfiguration)
		{
		}

		public RegisterRoutesTask(Action<RouteCollection> runBefore, IRegisterAreas registerAreas, IGlobalConfiguration globalConfiguration)
		{
			_runBefore = runBefore;
			_registerAreas = registerAreas;
			_globalConfiguration = globalConfiguration;
		}

		public Task Execute(IAppBuilder application)
		{
			_globalConfiguration.Configure(registerWebApiRoutes);
			registerRoutes(RouteTable.Routes);
			return Task.FromResult(true);
		}

		private static void registerWebApiRoutes(HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();
		}

		public void registerRoutes(RouteCollection routes)
		{
			_runBefore(routes);

			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.IgnoreRoute("content/{*pathInfo}");

            routes.IgnoreRoute("favicon.ico");

			var mapRoute = routes.MapRoute(
				"Root-authentication",
				"Authentication/{action}",
				new {controller = "Authentication", action = "SignIn", area = "Start"},
				null,
				new[] {"Teleopti.Ccc.Web.Areas.Start.*"});
			mapRoute.DataTokens["area"] = "Start";

			registerAreas();

			mapRoute = routes.MapRoute(
				"Default",
				// Route name
				"{controller}/{action}/{id}",
				// URL with parameters
				new { controller = "Authentication", action = "Index", id = UrlParameter.Optional },
				null,
				new[] { "Teleopti.Ccc.Web.Areas.Start.*" });
			mapRoute.DataTokens["area"] = "Start"; // Parameter defaults
		}

		private void registerAreas()
		{
			if (_registerAreas == null) return;

			_registerAreas.Execute();
		}
	}
}