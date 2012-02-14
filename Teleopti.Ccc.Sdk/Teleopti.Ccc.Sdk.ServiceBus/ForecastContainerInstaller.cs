using Autofac;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class ForecastContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>();
			builder.RegisterType<WorkloadDayHelper>();
		}
    }
}