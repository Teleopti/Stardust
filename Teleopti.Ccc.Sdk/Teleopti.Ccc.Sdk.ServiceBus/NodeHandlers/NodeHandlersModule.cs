using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class NodeHandlersModule :Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ImportForecastFromFileHandler>().As<IHandle<ImportForecastsFileToSkill>>();
		}
	}
}