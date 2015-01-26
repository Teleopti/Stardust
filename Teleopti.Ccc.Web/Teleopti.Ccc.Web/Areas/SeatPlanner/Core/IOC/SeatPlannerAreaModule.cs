using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers;
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

		}
	}
}

