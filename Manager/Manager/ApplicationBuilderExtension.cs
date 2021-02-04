#if NETSTANDARD
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Internal;

namespace Stardust.Manager
{
	public static class ApplicationBuilderExtension
	{
        public static void UseStardustManager(this IApplicationBuilder appBuilder,
		                                      ManagerConfiguration managerConfiguration,
		                                      ILifetimeScope lifetimeScope)
		{
			appBuilder.Map(
				managerConfiguration.Route,
				inner =>
                {
                    inner.UseEndpoint();
                });

			var builder = new ContainerBuilder();
			builder.RegisterModule(new ManagerModule(managerConfiguration));
            var container = builder.Build();

            //to start the timers etc
            container.Resolve<ManagerController>();
		}
	}
}
#endif