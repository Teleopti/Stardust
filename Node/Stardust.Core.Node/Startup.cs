using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
//using NodeTest.JobHandlers;

//using NodeTest.JobHandlers;

//using NodeTest.JobHandlers;

namespace Stardust.Core.Node
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IContainer ApplicationContainer;

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            //NodeModule.RegisterNodeServices(services);
            services.Add(new ServiceDescriptor(typeof(ILoggerFactory), new LoggerFactory()));
            services.Add(new ServiceDescriptor(typeof(ILoggerFactory), typeof(LoggerFactory),ServiceLifetime.Scoped));

            var containerBuilder = new ContainerBuilder();
            //containerBuilder.RegisterInstance(nodeConfigurationService);
            //containerBuilder.RegisterModule(new WorkerModule());
            containerBuilder.RegisterModule<NodeModule>();
            containerBuilder.Populate(services);
            ApplicationContainer = containerBuilder.Build();
            return new AutofacServiceProvider(ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseMvc();
            //app.()

            // Configure Web API for self-host. 
            //var config = new HttpConfiguration
            //{
            //    DependencyResolver = new AutofacWebApiDependencyResolver(container)
            //};

            //config.Services.Replace(typeof(IAssembliesResolver), new SlimAssembliesResolver(typeof(SlimAssembliesResolver).Assembly));
            //config.MapHttpAttributeRoutes();
            //config.Services.Add(typeof(IExceptionLogger),
            //                    new GlobalExceptionLogger());

            //appBuilder.UseAutofacMiddleware(container);
            //appBuilder.UseAutofacWebApi(config);
            //appBuilder.UseWebApi(config);
        }
    }
}
