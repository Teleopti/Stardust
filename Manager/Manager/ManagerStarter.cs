using System;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using Microsoft.Owin.Hosting;
using Owin;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;
using Stardust.Manager.Helpers;

namespace Stardust.Manager
{
	public class ManagerStarter
	{
        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

	    public void Stop()
	    {
	        QuitEvent.Set();
	    }

        private string WhoAmI;
		public void Start(ManagerConfiguration managerConfiguration)
		{
			WhoAmI = "[MANAGER, " + Environment.MachineName.ToUpper() + "]";
			using (WebApp.Start(managerConfiguration.BaseAdress,
                appBuilder =>
					 {
						 var builder = new ContainerBuilder();
						 builder.RegisterType<NodeManager>().As<INodeManager>();
						 builder.RegisterType<JobManager>();
						 builder.RegisterType<HttpSender>().As<IHttpSender>();

						 builder.Register(
							 c => new JobRepository(managerConfiguration.ConnectionString))
							 .As<IJobRepository>();

						 builder.Register(
							 c => new WorkerNodeRepository(managerConfiguration.ConnectionString))
							 .As<IWorkerNodeRepository>();

						 builder.RegisterApiControllers(typeof(ManagerController).Assembly);

						builder.RegisterInstance(managerConfiguration);

						 var container = builder.Build();

						 // Configure Web API for self-host. 
						 var config = new HttpConfiguration
						 {
							 DependencyResolver = new AutofacWebApiDependencyResolver(container)
						 };
						 config.MapHttpAttributeRoutes();
						 config.Services.Add(typeof(IExceptionLogger),
							  new GlobalExceptionLogger());
						 appBuilder.UseAutofacMiddleware(container);
						 appBuilder.UseAutofacWebApi(config);
						 appBuilder.UseWebApi(config);
					 }))

            {
                LogHelper.LogInfoWithLineNumber(WhoAmI + ": Started listening on port : " + managerConfiguration.BaseAdress);

                QuitEvent.WaitOne();

			}
		}
	}
}
