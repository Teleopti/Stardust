using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using Owin;

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

#pragma warning disable 618
            builder.Update(lifetimeScope.ComponentRegistry);
#pragma warning restore 618

            //to start the timers etc
			lifetimeScope.Resolve<ManagerController>();
		}
	}
}