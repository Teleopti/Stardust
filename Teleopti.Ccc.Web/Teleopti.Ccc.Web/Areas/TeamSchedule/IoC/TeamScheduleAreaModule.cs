using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.IoC
{
	public class TeamScheduleAreaModule:Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TeamScheduleProjectionProvider>().As<ITeamScheduleProjectionProvider>().SingleInstance();
			builder.RegisterType<TeamScheduleViewModelFactory>().As<ITeamScheduleViewModelFactory>().SingleInstance();

		}
	}
}