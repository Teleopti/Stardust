using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(2)]
	public class RegisterRoutesTask : IBootstrapperTask
	{
		private readonly IRegisterAreas _registerAreas;
		private readonly Action<RouteCollection> _runBefore;

		public RegisterRoutesTask(IRegisterAreas registerAreas) : this(r => { })
		{
			_registerAreas = registerAreas;
		}

		public RegisterRoutesTask(Action<RouteCollection> runBefore)
		{
			_runBefore = runBefore;
		}

		public Task Execute()
		{
			registerRoutes(RouteTable.Routes);
			return null;
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

			_registerAreas.Execute();

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
	}
}