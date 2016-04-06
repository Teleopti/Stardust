using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class ForecastContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>();
			builder.RegisterType<WorkloadDayHelper>();
			builder.RegisterType<WorkloadDayHelper>().As<IWorkloadDayHelper>();
			builder.Register(c => StatisticRepositoryFactory.Create()).As<IStatisticRepository>();
			builder.RegisterType<ForecastClassesCreator>().As<IForecastClassesCreator>();
		    
        }
    }
}