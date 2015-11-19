﻿using Autofac;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.IOC
{
	public class SeatPlannerAreaModule : Module
	{

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ExceptionHandlerPipelineModule>().As<IHubPipelineModule>();
			builder.RegisterType<TeamsProvider>().As<ITeamsProvider>().InstancePerLifetimeScope();
			builder.RegisterType<SeatMapProvider>().As<ISeatMapProvider>().InstancePerLifetimeScope();
			builder.RegisterType<SeatPlanProvider>().As<ISeatPlanProvider>().InstancePerLifetimeScope();
			builder.RegisterType<SeatBookingReportProvider>().As<ISeatBookingReportProvider>().InstancePerLifetimeScope();
			builder.RegisterType<LocationHierarchyProvider>().As<ILocationHierarchyProvider>().InstancePerLifetimeScope();
			builder.RegisterType<SeatOccupancyProvider>().As<ISeatOccupancyProvider>().InstancePerLifetimeScope();
			builder.RegisterType<SeatPlanPersister>().As<ISeatPlanPersister>().InstancePerLifetimeScope();
			builder.RegisterType<Domain.SeatPlanning.SeatPlanner>().As<ISeatPlanner>().InstancePerLifetimeScope();
			builder.RegisterType<SeatMapPersister>().As<ISeatMapPersister>().InstancePerLifetimeScope();
			builder.RegisterType<SeatBookingRequestAssembler>().As<ISeatBookingRequestAssembler>().InstancePerLifetimeScope();
			
		}
	}
}

