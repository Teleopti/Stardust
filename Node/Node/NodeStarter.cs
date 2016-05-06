using System;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using Microsoft.Owin.Hosting;
using Owin;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

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
				throw new ArgumentNullException("nodeConfiguration");
			}
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			var nodeAddress = nodeConfiguration.BaseAddress.Scheme +
			                  "://+:" +
			                  nodeConfiguration.BaseAddress.Port + "/";

			using (WebApp.Start(nodeAddress,
			                    appBuilder =>
			                    {
									//string owinListenerName = "Microsoft.Owin.Host.HttpListener.OwinHttpListener";
									//OwinHttpListener owinListener = (OwinHttpListener)appBuilder.Properties[owinListenerName];

									//int maxAccepts;
									//int maxRequests;
									//owinListener.GetRequestProcessingLimits(out maxAccepts, out maxRequests);

									//owinListener.SetRequestQueueLimit(int.MaxValue);
									//owinListener.SetRequestProcessingLimits(int.MaxValue, int.MaxValue);

									var containerBuilder = new ContainerBuilder();

									//containerBuilder.RegisterInstance(nodeConfiguration);
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
				WhoAmI = nodeConfiguration.CreateWhoIAm(Environment.MachineName);

				_logger.InfoWithLineNumber(WhoAmI + ": Node started on machine.");

				_logger.InfoWithLineNumber(WhoAmI + ": Listening on port " + nodeConfiguration.BaseAddress);

				_quitEvent.WaitOne();
			}
		}
	}
}