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
		                                      ManagerConfiguration managerConfiguration,
		                                      ILifetimeScope lifetimeScope)
		{
			appBuilder.Map(
				managerConfiguration.Route,
				inner =>
				{
					var config = new HttpConfiguration
					{
						DependencyResolver = new AutofacWebApiDependencyResolver(lifetimeScope)
					};

				    config.Services.Replace(typeof(IAssembliesResolver), new SlimAssembliesResolver(typeof(SlimAssembliesResolver).Assembly));
                    config.MapHttpAttributeRoutes();

					config.Services.Add(typeof (IExceptionLogger),
					                    new GlobalExceptionLogger());

					inner.UseAutofacWebApi(config);
					inner.UseWebApi(config);
				});

			var builder = new ContainerBuilder();
			builder.RegisterModule(new ManagerModule(managerConfiguration));
#pragma warning disable CS0618 // Type or member is obsolete
            builder.Update(lifetimeScope.ComponentRegistry);
#pragma warning restore CS0618 // Type or member is obsolete

            //to start the timers etc
            lifetimeScope.Resolve<ManagerController>();
		}
	}
}
#endif