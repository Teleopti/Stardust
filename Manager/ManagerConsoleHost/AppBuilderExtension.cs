using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using Owin;
using Stardust.Manager;

namespace ManagerConsoleHost
{
    public static class AppBuilderExtension
    {

        public static void UseStardustManager(this IAppBuilder builder, IContainer container, string routeName)
        {
            var config = new HttpConfiguration();
            
            config.Routes.MapHttpRoute(
                name: "Manager",
                routeTemplate: "{controller}/{action}/{jobId}",
                defaults: new {controller = routeName, jobId = RouteParameter.Optional}
                );

            config.Routes.MapHttpRoute(
                name: "Manager2",
                routeTemplate: "{controller}/status/{action}/{jobId}",
                defaults: new {controller = routeName, jobId = RouteParameter.Optional }
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

            config.Services.Add(typeof (IExceptionLogger),
                new GlobalExceptionLogger());

            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            builder.UseAutofacMiddleware(container);
            builder.UseAutofacWebApi(config);
            builder.UseWebApi(config);
        }
    }
}
