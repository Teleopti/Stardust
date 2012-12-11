﻿using System;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(2)]
	public class RegisterRoutesTask : IBootstrapperTask
	{
		private readonly Action _registerAllAreas;

		public RegisterRoutesTask() : this(AreaRegistration.RegisterAllAreas)
		{
		}

		public RegisterRoutesTask(Action registerAllAreas)
		{
			_registerAllAreas = registerAllAreas;
		}

		public void Execute()
		{
			registerRoutes(RouteTable.Routes);
		}

		public void registerRoutes(RouteCollection routes)
		{
			//routes.Clear();

			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.IgnoreRoute("content/{*pathInfo}");

			var mapRoute = routes.MapRoute(
				"Root-authentication",
				"Authentication/{action}",
				new {controller = "Authentication", action = "SignIn", area = "Start"},
				null,
				new[] {"Teleopti.Ccc.Web.Areas.Start.*"});
			mapRoute.DataTokens["area"] = "Start";

			_registerAllAreas();

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