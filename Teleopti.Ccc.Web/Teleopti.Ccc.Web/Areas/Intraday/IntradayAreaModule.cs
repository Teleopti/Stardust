﻿using Autofac;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	public class IntradayAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<IntradaySkillStatusService>().As<IIntradaySkillStatusService>().InstancePerLifetimeScope();
			builder.RegisterType<SkillForecastedTasksProvider>().As<ISkillForecastedTasksProvider>().InstancePerLifetimeScope();
			builder.RegisterType<SkillActualTasksProvider>().As<ISkillActualTasksProvider>().InstancePerLifetimeScope();
		}
	}
}