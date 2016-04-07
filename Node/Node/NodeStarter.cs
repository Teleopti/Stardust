using System;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using Autofac.Integration.WebApi;
using Castle.DynamicProxy;
using log4net;
using Microsoft.Owin.Hosting;
using Owin;
using Stardust.Node.API;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Log4Net.Extensions;
using Stardust.Node.Log4Net.Interceptors;
using Stardust.Node.Timers;
using Stardust.Node.Workers;

namespace Stardust.Node
{
	public class NodeStarter : INodeStarter
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (NodeStarter));

		private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

		private string WhoAmI { get; set; }

		public void Stop()
		{
			QuitEvent.Set();
		}

		public void Start(INodeConfiguration nodeConfiguration,
		                  IContainer container)
		{
			var nodeAddress = nodeConfiguration.BaseAddress.Scheme +
			                  "://+:" +
			                  nodeConfiguration.BaseAddress.Port + "/";

			// Start OWIN host 
			using (WebApp.Start(nodeAddress,
			                    appBuilder =>
			                    {
				                    var containerBuilder = new ContainerBuilder();

				                    containerBuilder.RegisterType<Log4NetInterceptor>().Named<IInterceptor>("log-calls");

				                    containerBuilder.RegisterType<HttpSender>().As<IHttpSender>();

				                    containerBuilder.RegisterType<InvokeHandler>()
					                    .SingleInstance().EnableClassInterceptors();

				                    containerBuilder.RegisterType<NodeController>()
					                    .SingleInstance();

				                    containerBuilder.RegisterApiControllers(typeof (NodeController).Assembly)
					                    .EnableClassInterceptors();

				                    containerBuilder.RegisterInstance(nodeConfiguration);

				                    containerBuilder
					                    .Register(context => new TrySendJobProgressToManagerTimer(nodeConfiguration,
					                                                                              context.Resolve<IHttpSender>(),
					                                                                              5000))
					                    .SingleInstance();


				                    // Register IWorkerWrapper.
				                    containerBuilder
					                    .Register<IWorkerWrapper>(c => new WorkerWrapper(c.Resolve<InvokeHandler>(),
					                                                                     nodeConfiguration,
					                                                                     new TrySendNodeStartUpNotificationToManagerTimer
						                                                                     (nodeConfiguration,
						                                                                      nodeConfiguration
							                                                                      .GetManagerNodeHasBeenInitializedUri(),
						                                                                      c.Resolve<IHttpSender>()),
					                                                                     new PingToManagerTimer(nodeConfiguration,
					                                                                                            nodeConfiguration
						                                                                                            .GetManagerNodeHeartbeatUri
						                                                                                            (),
					                                                                                            c.Resolve<IHttpSender>()),
					                                                                     new TrySendJobDoneStatusToManagerTimer
						                                                                     (nodeConfiguration,
						                                                                      c.Resolve<TrySendJobProgressToManagerTimer>(),
						                                                                      c.Resolve<IHttpSender>()),
					                                                                     new TrySendJobCanceledToManagerTimer
						                                                                     (nodeConfiguration,
						                                                                      c.Resolve<TrySendJobProgressToManagerTimer>(),
						                                                                      c.Resolve<IHttpSender>()),
					                                                                     new TrySendJobFaultedToManagerTimer
						                                                                     (nodeConfiguration,
						                                                                      c.Resolve<TrySendJobProgressToManagerTimer>(),
						                                                                      c.Resolve<IHttpSender>()),
					                                                                     c.Resolve<TrySendJobProgressToManagerTimer>()))
					                    .SingleInstance();


				                    containerBuilder.Update(container);

				                    //to start it
				                    container.Resolve<IWorkerWrapper>();

				                    // Configure Web API for self-host. 
				                    var config = new HttpConfiguration
				                    {
					                    DependencyResolver = new AutofacWebApiDependencyResolver(container)
				                    };

				                    config.MapHttpAttributeRoutes();
				                    config.Services.Add(typeof (IExceptionLogger),
				                                        new GlobalExceptionLogger());

				                    appBuilder.UseAutofacMiddleware(container);
				                    appBuilder.UseAutofacWebApi(config);
				                    appBuilder.UseWebApi(config);
			                    }))

			{
				WhoAmI = nodeConfiguration.CreateWhoIAm(Environment.MachineName);

				Logger.InfoWithLineNumber(WhoAmI + ": Node started on machine.");

				Logger.InfoWithLineNumber(WhoAmI + ": Listening on port " + nodeConfiguration.BaseAddress);

				QuitEvent.WaitOne();
			}
		}
	}
}