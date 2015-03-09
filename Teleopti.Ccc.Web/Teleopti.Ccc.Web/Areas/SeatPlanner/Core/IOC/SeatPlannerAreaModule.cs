using Autofac;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.IOC
{
	public class SeatPlannerAreaModule : Module
	{

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ExceptionHandlerPipelineModule>().As<IHubPipelineModule>();
			builder.RegisterType<TeamsProvider>().As<ITeamsProvider>().InstancePerLifetimeScope();
			builder.RegisterType<SeatMapProvider>().As<ISeatMapProvider>().InstancePerLifetimeScope();
			builder.RegisterType<LocationHierarchyProvider>().As<ILocationHierarchyProvider>().InstancePerLifetimeScope();
		}
	}
}

