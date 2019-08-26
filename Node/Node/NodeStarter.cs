using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stardust.Node.Extensions;
using Stardust.Node.Workers;

namespace Stardust.Node
{
	public class NodeStarter
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof (NodeStarter));

		private readonly CancellationTokenSource _quitEvent = new CancellationTokenSource();

		private string WhoAmI { get; set; }

		public void Stop()
		{
            _logger.InfoWithLineNumber("Stopping the node.");
            _quitEvent.Cancel();
		}

        public async Task Start(NodeConfiguration nodeConfiguration,
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

            var nodeAddress = $"http://+:{nodeConfiguration.BaseAddress.Port}/";
            IWebHostBuilder builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAutofac();
                    services.AddSingleton(new AutofacContainerWrapper(container));
                })
                .ConfigureAppConfiguration(cfg => cfg.Properties.Add("baseAutofacContainer", container))
                .UseUrls(nodeAddress)
                .UseStartup<Startup>()
                .UseKestrel();
            
            using (var host = builder.Build())
            {
                host.Start();

                WhoAmI = nodeConfiguration.CreateWhoIAm(nodeConfiguration.BaseAddress.LocalPath);

                _logger.InfoWithLineNumber($"{WhoAmI}: Node starting on machine.");

                _logger.InfoWithLineNumber($"{WhoAmI}: Listening on port {nodeConfiguration.BaseAddress}");

                container.Resolve<WorkerWrapperService>().GetWorkerWrapperByPort(nodeConfiguration.BaseAddress.Port);

                _logger.InfoWithLineNumber("Use Ctrl-C to shutdown the host...");
                await host.WaitForShutdownAsync(_quitEvent.Token);
            }

            _logger.InfoWithLineNumber($"{WhoAmI}: Stopped listening on port {nodeConfiguration.BaseAddress}");
        }

        public class Startup
        {
            private readonly AutofacContainerWrapper _wrapper;

            public Startup(IHostingEnvironment env, AutofacContainerWrapper wrapper)
            {
                _wrapper = wrapper;
                var builder = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddEnvironmentVariables();

                Configuration = builder.Build();
            }

            public IConfigurationRoot Configuration { get; private set; }

            // ConfigureServices is where you register dependencies. This gets
            // called by the runtime before the ConfigureContainer method, below.
            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                // Add services to the collection. Don't build or return
                // any IServiceProvider or the ConfigureContainer method
                // won't get called.
                services.AddMvc();

                // Create the container builder.
                var builder = new ContainerBuilder();

                // Register dependencies, populate the services from
                // the collection, and build the container.
                //
                // Note that Populate is basically a foreach to add things
                // into Autofac that are in the collection. If you register
                // things in Autofac BEFORE Populate then the stuff in the
                // ServiceCollection can override those things; if you register
                // AFTER Populate those registrations can override things
                // in the ServiceCollection. Mix and match as needed.
                builder.Populate(services);
#pragma warning disable 618
                builder.Update(_wrapper.Container);
#pragma warning restore 618

                ApplicationContainer = _wrapper.Container;

                // Create the IServiceProvider based on the container.
                return new AutofacServiceProvider(this.ApplicationContainer);
            }

            public IContainer ApplicationContainer { get; private set; }

            public void Configure(IApplicationBuilder app)
            {
                app.UseMvc();
            }
        }
    }

    public class AutofacContainerWrapper
    {
        public IContainer Container { get; }

        public AutofacContainerWrapper(IContainer container)
        {
            Container = container;
        }
    }
}