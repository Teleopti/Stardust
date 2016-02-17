using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public static class AppBuilderExtension
	{

		public static void UseStardustManager(this IAppBuilder appBuilder, ManagerConfiguration managerConfiguration, ILifetimeScope lifetimeScope)
		{
            appBuilder.UseDefaultFiles();
            appBuilder.UseStaticFiles();

            string routeName = managerConfiguration.routeName;
            
            var config = new HttpConfiguration();

			config.Routes.MapHttpRoute(
				 name: "Manager",
				 routeTemplate: "{controller}/{action}/{jobId}",
				 defaults: new { controller = routeName, jobId = RouteParameter.Optional }
				 );

            config.Routes.MapHttpRoute(
                 name: "Manager2",
                 routeTemplate: "{controller}/status/{action}/{jobId}",
                 defaults: new { controller = routeName, jobId = RouteParameter.Optional }
                 );

            config.Routes.MapHttpRoute(
                 name: "Manager3",
                 routeTemplate: "{controller}/{action}/{model}",
                 defaults: new { controller = routeName }
                 );

            config.Routes.MapHttpRoute(
                name: "Manager4",
                routeTemplate: "{controller}/{action}/{nodeUri}",
                defaults: new { controller = routeName }
                );

            config.Services.Add(typeof(IExceptionLogger),
				 new GlobalExceptionLogger());

			config.DependencyResolver = new AutofacWebApiDependencyResolver(lifetimeScope);
			appBuilder.UseAutofacMiddleware(lifetimeScope);
			appBuilder.UseAutofacWebApi(config);
			appBuilder.UseWebApi(config);

			
		}
	}
}
