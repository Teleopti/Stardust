using Autofac;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class OutboundScheduledResourcesProviderModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<OutboundScheduledResourcesProvider>().As<IOutboundScheduledResourcesProvider>();
			builder.RegisterType<OutboundAssignedStaffProvider>().As<OutboundAssignedStaffProvider>();
			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>().InstancePerLifetimeScope();
		}
	}
}