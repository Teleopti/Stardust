using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Stardust.Manager;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerConsoleHost
{
    public static class AppBuilderExtension
    {

        public static void UseStardustManager(this IAppBuilder appBuilder, ManagerConfiguration managerConfiguration)
        {
            string routeName = managerConfiguration.routeName;

            var builder = new ContainerBuilder();

            builder.RegisterType<NodeManager>()
                .As<INodeManager>();

            builder.RegisterType<JobManager>();

            builder.RegisterType<HttpSender>()
                .As<IHttpSender>();

            builder.Register(
                c => new JobRepository(managerConfiguration.ConnectionString))
                .As<IJobRepository>();

            builder.Register(
                c => new WorkerNodeRepository(managerConfiguration.ConnectionString))
                .As<IWorkerNodeRepository>();

            builder.RegisterApiControllers(typeof(ManagerController).Assembly);

            builder.RegisterInstance(managerConfiguration);

            var container = builder.Build();

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
            appBuilder.UseAutofacMiddleware(container);
            appBuilder.UseAutofacWebApi(config);
            appBuilder.UseWebApi(config);

            appBuilder.UseDefaultFiles(new DefaultFilesOptions
            {
                FileSystem = new PhysicalFileSystem(@".\StardustDashboard"),
                RequestPath = new PathString("/StardustDashboard")
            });

            appBuilder.UseStaticFiles();
        }
    }
}
