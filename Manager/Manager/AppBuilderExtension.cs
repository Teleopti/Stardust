#if NET472
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using Autofac.Integration.WebApi;
using Owin;
using Autofac;

namespace Stardust.Manager
{
	public static class AppBuilderExtension
	{
        public static void UseStardustManager(this IAppBuilder appBuilder,
		                                      ManagerConfiguration managerConfiguration)
		{
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ManagerModule(managerConfiguration));
            var container = builder.Build();

			appBuilder.Map(
				managerConfiguration.Route,
				inner =>
				{
					var config = new HttpConfiguration
					{
						DependencyResolver = new AutofacWebApiDependencyResolver(container)
					};

				    config.Services.Replace(typeof(IAssembliesResolver), new SlimAssembliesResolver(typeof(SlimAssembliesResolver).Assembly));
                    config.MapHttpAttributeRoutes();

					config.Services.Add(typeof (IExceptionLogger),
					                    new GlobalExceptionLogger());

					inner.UseAutofacMiddleware(container);
					inner.UseAutofacWebApi(config);
					inner.UseWebApi(config);
				});


            //to start the timers etc
            container.Resolve<ManagerController>();
		}
	}
}
#endif