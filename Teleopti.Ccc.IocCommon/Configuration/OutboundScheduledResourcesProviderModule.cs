using Autofac;
using Teleopti.Ccc.Domain.Outbound;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class OutboundScheduledResourcesProviderModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<OutboundAssignedStaffProvider>().As<OutboundAssignedStaffProvider>();
		}
	}
}