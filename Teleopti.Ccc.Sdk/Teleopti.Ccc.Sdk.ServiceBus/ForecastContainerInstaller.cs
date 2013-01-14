﻿using Autofac;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class ForecastContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>();
			builder.RegisterType<WorkloadDayHelper>();
			builder.Register(c => StatisticRepositoryFactory.Create()).As<IStatisticRepository>();
			builder.RegisterType<StatisticLoader>().As<IStatisticLoader>();
			builder.RegisterType<ReforecastPercentCalculator>().As<IReforecastPercentCalculator>();
			builder.RegisterType<Statistic>().As<IStatistic>();
		}
    }
}