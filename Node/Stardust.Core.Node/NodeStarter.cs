using System;
using System.Threading;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
//using System.Web.Http;
//using System.Web.Http.Dispatcher;
//using System.Web.Http.ExceptionHandling;
//using Autofac;
//using Autofac.Integration.WebApi;
using Microsoft.Extensions.Logging;
//using log4net;
//using Microsoft.Owin.Hosting;
//using Owin;
using Stardust.Core.Node.Extensions;
using Microsoft.Extensions.DependencyInjection;
//using NodeTest.JobHandlers;
using Stardust.Core.Node.Interfaces;
//using NodeTest.JobHandlers;
using WorkerWrapperService = Stardust.Core.Node.Workers.WorkerWrapperService;

namespace Stardust.Core.Node
{
	public class NodeStarter
	{
		private readonly ILogger _logger = new LoggerFactory().CreateLogger(typeof (NodeStarter));

		private readonly ManualResetEvent _quitEvent = new ManualResetEvent(false);

		private string WhoAmI { get; set; }

		public void Stop()
		{
			_quitEvent.Set();
		}

		public void Start(NodeConfiguration nodeConfiguration,
            ContainerBuilder containerBuilder, IModule workerModule)
		{
			if (nodeConfiguration == null)
			{
				throw new ArgumentNullException(nameof(nodeConfiguration));
			}
			if (containerBuilder == null)
			{
				throw new ArgumentNullException(nameof(containerBuilder));
			}

            var container = containerBuilder.Build();
            var nodeAddress = "http://+:" + nodeConfiguration.BaseAddress.Port + "/";

            //var config = new

            //new Startup(nodeConfiguration).Configure(
            //   );
            //using (
            //WebHost.Start(nodeAddress,
            //                appBuilder =>
            //                {
            //                    // Configure Web API for self-host. 
            //                    var config = new HttpConfiguration
            //                    {
            //                        DependencyResolver = new AutofacWebApiDependencyResolver(container)
            //                    };

            //                    config.Services.Replace(typeof(IAssembliesResolver), new SlimAssembliesResolver(typeof(SlimAssembliesResolver).Assembly));
            //                    config.MapHttpAttributeRoutes();
            //                    config.Services.Add(typeof(IExceptionLogger),
            //                                        new GlobalExceptionLogger());

            //                    appBuilder.UseAutofacMiddleware(container);
            //                    appBuilder.UseAutofacWebApi(config);
            //                    appBuilder.UseWebApi(config);
            //                })
            //              )

            //{
            //var startup = new Startup(nodeConfiguration);
            //startup.ConfigureServices(new ServiceCollection());
            //startup.Configure();

            //var factory = new AutofacServiceProviderFactory(builder => builder.Build());
            //var provider = factory.CreateServiceProvider(containerBuilder);
            //provider.GetService()

            var webHost = CreateWebHostBuilder(new[] {""})
                //.UseSetting("http_port", nodeConfiguration.BaseAddress.Port.ToString())
                //.UseSetting("https_port", 14100.ToString()
                .UseUrls("http://localhost:14100", "https://localhost:14101")
                //.ConfigureServices()
                //.ConfigureServices(collection =>
                //{ 
                //    containerBuilder.Populate(collection);
                //    container = containerBuilder.Build();
                //})

                .ConfigureServices(services =>
                    {
                        services.AddSingleton(container.Resolve<NodeConfigurationService>());
                        //services.AddScoped<Stardust.Node.Interfaces.IHandle<TestJobParams>, TestJobWorker>();
                        //containerBuilder.Populate(services);
                    })
                .Build();

            //builder.RegisterType<TestJobWorker>().As<IHandle<TestJobParams>>();
            //var container = containerBuilder.Build();
            //container.
            var task = webHost.RunAsync();
            
            WhoAmI = nodeConfiguration.CreateWhoIAm(nodeConfiguration.BaseAddress.LocalPath);

            _logger.InfoWithLineNumber(WhoAmI + ": Node started on machine.");

            _logger.InfoWithLineNumber(WhoAmI + ": Listening on port " + nodeConfiguration.BaseAddress);

            //to start it
            //container.Resolve<NodeController>();
            //container.Resolve<WorkerWrapperService>().GetWorkerWrapperByPort(nodeConfiguration.BaseAddress.Port);
            webHost.Services.GetService<WorkerWrapperService>()
                .GetWorkerWrapperByPort(nodeConfiguration.BaseAddress.Port);
            //serviceProvider.GetService<WorkerWrapperService>().GetWorkerWrapperByPort(nodeConfiguration.BaseAddress.Port);

            //nodeController.Init(nodeConfiguration);
          

            _quitEvent.WaitOne();
           // }
		}

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}