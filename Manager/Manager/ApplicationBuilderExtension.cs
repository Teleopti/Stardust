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
#pragma warning disable CS0618 // Type or member is obsolete
            builder.Update(lifetimeScope.ComponentRegistry);
#pragma warning restore CS0618 // Type or member is obsolete

            //to start the timers etc
            lifetimeScope.Resolve<ManagerController>();
		}
	}
}
#endif