using System;
using System.Diagnostics;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using Microsoft.Owin.Hosting;
using Owin;
using Stardust.Node.Extensions;

namespace Stardust.Node
{
	public class NodeStarter
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof (NodeStarter));

		private readonly ManualResetEvent _quitEvent = new ManualResetEvent(false);

		private string WhoAmI { get; set; }

		public void Stop()
		{
			_quitEvent.Set();
		}

		public void Start(NodeConfiguration nodeConfiguration,
		                  IContainer container)
		{
			if (nodeConfiguration == null)
			{
				throw new ArgumentNullException(nameof(nodeConfiguration));
			}
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}
			
			var nodeAddress = "http://+:" + nodeConfiguration.BaseAddress.Port + "/";

			using (WebApp.Start(nodeAddress,
			                    appBuilder =>
			                    {
									var containerBuilder = new ContainerBuilder();
									containerBuilder.RegisterModule(new NodeModule(nodeConfiguration));
									containerBuilder.Update(container);

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
				WhoAmI = nodeConfiguration.CreateWhoIAm(nodeConfiguration.BaseAddress.LocalPath);

				_logger.InfoWithLineNumber(WhoAmI + ": Node started on machine.");

				_logger.InfoWithLineNumber(WhoAmI + ": Listening on port " + nodeConfiguration.BaseAddress);

				//to start it
				container.Resolve<NodeController>();

				//set process priority class to BelowNormal
				if (nodeConfiguration.PriorityBelowNormal)
				{
					Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
				}

				_quitEvent.WaitOne();
			}
		}
	}
}