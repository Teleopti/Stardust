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
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Stardust.Node.Log4Net;
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
			var nodeAddress = nodeConfiguration.BaseAddress.Scheme + "://+:" + nodeConfiguration.BaseAddress.Port + "/";

			// Start OWIN host 
			using (WebApp.Start(nodeAddress,
			                    appBuilder =>
			                    {
				                    var containerBuilder = new ContainerBuilder();

									containerBuilder.RegisterType<Log4NetInterceptor>().Named<IInterceptor>("log-calls");

									containerBuilder.RegisterType<HttpSender>().EnableClassInterceptors();
									
									var httpSenderInstance = new HttpSender();

				                    containerBuilder.RegisterInstance(httpSenderInstance);

				                    containerBuilder.RegisterType<InvokeHandler>()
					                    .SingleInstance();
									

									containerBuilder.RegisterType<NodeController>()
					                    .SingleInstance().EnableClassInterceptors();

				                    containerBuilder.RegisterApiControllers(typeof (NodeController).Assembly);

				                    containerBuilder.RegisterInstance(nodeConfiguration);

				                    var trySendJobProgressToManagerTimer = new TrySendJobProgressToManagerTimer(nodeConfiguration,
				                                                                                                new HttpSender(), 
				                                                                                                5000);

				                    containerBuilder.RegisterInstance(trySendJobProgressToManagerTimer).SingleInstance();


									// Register IWorkerWrapper.
									containerBuilder.Register<IWorkerWrapper>(c => new WorkerWrapper(c.Resolve<InvokeHandler>(),
				                                                                            nodeConfiguration,
				                                                                            new TrySendNodeStartUpNotificationToManagerTimer
					                                                                            (nodeConfiguration,
																								nodeConfiguration.GetManagerNodeHasBeenInitializedUri()),
				                                                                            new PingToManagerTimer(nodeConfiguration,
																													nodeConfiguration.GetManagerNodeHeartbeatUri()),
				                                                                            new TrySendJobDoneStatusToManagerTimer(nodeConfiguration, trySendJobProgressToManagerTimer),
				                                                                            new TrySendJobCanceledToManagerTimer(nodeConfiguration, trySendJobProgressToManagerTimer),
				                                                                            new TrySendJobFaultedToManagerTimer(nodeConfiguration, trySendJobProgressToManagerTimer),
																							trySendJobProgressToManagerTimer,
																							httpSenderInstance))
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