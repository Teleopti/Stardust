﻿using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class NodeHandlersModule :Module
	{
		private readonly IocConfiguration _configuration;

		public NodeHandlersModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterModule(new RuleSetModule(_configuration, true));

			builder.RegisterType<StardustHealthCheckHandler>().As<IHandle<StardustHealthCheckEvent>>().SingleInstance();
			builder.RegisterType<ImportForecastFromFileHandler>().As<IHandle<ImportForecastsFileToSkillEvent>>().SingleInstance();
			builder.RegisterType<ExportMultisiteSkillsToSkillEventHandler>().As<IHandle<ExportMultisiteSkillsToSkillEvent>>().SingleInstance();
			builder.RegisterType<RunSchedulingHandler>().As<IHandle<ScheduleOnNode>>().SingleInstance().ApplyAspects();
			builder.RegisterType<AbsenceRequstHandler>().As<IHandle<NewAbsenceRequestCreatedEvent>>().SingleInstance().ApplyAspects();
			builder.RegisterType<SchedulingProgress>().As<ISchedulingProgress>().SingleInstance();
			builder.RegisterType<DataSourceScope>().As<IDataSourceScope>();
		}
	}
}