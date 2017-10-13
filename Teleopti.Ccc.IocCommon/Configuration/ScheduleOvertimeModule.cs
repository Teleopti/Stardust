﻿using Autofac;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Intraday;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ScheduleOvertimeModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public ScheduleOvertimeModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ScheduleOvertimeService>();
			builder.RegisterType<ScheduleOvertimeServiceWithoutStateholder>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleOvertimeWithoutStateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<CalculateBestOvertimeBeforeOrAfter>().InstancePerLifetimeScope();
			builder.RegisterType<OvertimeDateTimePeriodExtractor>().As<IOvertimeDateTimePeriodExtractor>();
			builder.RegisterType<OvertimeRelativeDifferenceCalculator>().SingleInstance();
			builder.RegisterType<AddOverTime>().InstancePerLifetimeScope();
			builder.RegisterType<PersonForOvertimeProvider>().As<IPersonForOvertimeProvider>().SingleInstance();
			builder.RegisterType<PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider>().SingleInstance();
			builder.RegisterType<PrimaryOrAllPersonSkillForNonOvertimeProvider>().SingleInstance();
			builder.RegisterType<PrimaryGroupPersonSkillAggregator>().SingleInstance();
			builder.RegisterType<PersonSkillsUseAllForScheduleDaysOvertimeProvider>().SingleInstance();
			builder.RegisterType<ScheduleOvertimeExecuteWrapper>().InstancePerLifetimeScope();
		}
	}
}
