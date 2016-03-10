using System;
using System.Collections.Generic;
using System.Web.Http;
using Mindscape.Raygun4Net.WebApi;

namespace Teleopti.Wfm.Administration
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
		{
			RaygunWebApiClient.Attach(config);
			// Web API configuration and services

			// Web API routes
			config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
