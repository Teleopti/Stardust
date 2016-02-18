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
        public static void UseStardustManager(this IAppBuilder appBuilder, ManagerConfiguration managerConfiguration,
            ILifetimeScope lifetimeScope)
        {
            appBuilder.Map(
                managerConfiguration.Route,
                inner =>
                {
                    var config = new HttpConfiguration();

                    config.DependencyResolver = new AutofacWebApiDependencyResolver(lifetimeScope);

                    config.MapHttpAttributeRoutes();

                    //config.Routes.MapHttpRoute("Manager", "{controller}/{action}/{jobId}",
                    //    new {action = "job", jobId = RouteParameter.Optional}
                    //    );

                    //config.Routes.MapHttpRoute("Manager2", "{controller}/status/{action}/{jobId}",
                    //    new {jobId = RouteParameter.Optional}
                    //    );

                    //config.Routes.MapHttpRoute("Manager3", "{controller}/{action}/{model}"
                    //    );

                    //config.Routes.MapHttpRoute("Manager4", "{controller}/{action}/{nodeUri}"
                    //    );

                    config.Services.Add(typeof (IExceptionLogger),
                        new GlobalExceptionLogger());


                    inner.UseAutofacWebApi(config);
                    inner.UseWebApi(config);
                });
        }
    }
}