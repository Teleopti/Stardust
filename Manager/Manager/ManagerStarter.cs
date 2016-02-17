using System.Threading;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class ManagerStarter
	{
		public void Start(ManagerConfiguration managerConfiguration, IContainer container, HttpConfiguration config)
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

			builder.Update(container);

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

			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
			//appBuilder.UseAutofacMiddleware(lifetimeScope);
			
			//appBuilder.UseWebApi(config);
		}
	}
}