using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Stardust.Node;
using Stardust.Node.Extensions;
using Stardust.Node.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Core.Stardust.Node
{
    public class NodeStarter
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(NodeStarter));
        private readonly CancellationTokenSource _quitEvent = new CancellationTokenSource();
        public string WhoAmI { get; set; }

        public IConfiguration Configuration { get; }
        public ILifetimeScope AutofacContainer { get; }

        public NodeStarter(IContainer container)
        {
            AutofacContainer = container;
        }


        public async Task Start(NodeConfiguration nodeConfiguration)
        {
            var nodeAddress = $"http://+:{nodeConfiguration.BaseAddress.Port}/";
            var hostBuilder =  Host.CreateDefaultBuilder()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.UseUrls(nodeAddress);
                    webHostBuilder.UseStartup<Startup>();
                    webHostBuilder.UseKestrel();
                });
            

            using (var host =  hostBuilder.Build())
            {
                host.Start();

                WhoAmI = nodeConfiguration.CreateWhoIAm(nodeConfiguration.BaseAddress.LocalPath);

                _logger.InfoWithLineNumber($"{WhoAmI}: Node starting on machine.");

                _logger.InfoWithLineNumber($"{WhoAmI}: Listening on port {nodeConfiguration.BaseAddress}");

                AutofacContainer.Resolve<WorkerWrapperService>().GetWorkerWrapperByPort(nodeConfiguration.BaseAddress.Port);

                _logger.InfoWithLineNumber("Use Ctrl-C to shutdown the host...");
                await host.WaitForShutdownAsync(_quitEvent.Token);
            }

            _logger.InfoWithLineNumber($"{WhoAmI}: Stopped listening on port {nodeConfiguration.BaseAddress}");
        }

        public void Stop()
        {
            _logger.InfoWithLineNumber("Stopping the node.");
            _quitEvent.Cancel();
        }


        public class Startup
        {
            public IConfigurationRoot Configuration { get; }

            public Startup(IHostEnvironment hostEnvironment)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(hostEnvironment.ContentRootPath)
                    .AddEnvironmentVariables();
                Configuration = builder.Build();
            }

            // This method gets called by the runtime. Use this method to add services to the container.
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddControllers();
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseHttpsRedirection();
                app.UseRouting();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }
        }

        


    }
}
