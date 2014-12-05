using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class CommandDispatcherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SyncCommandDispatcher>().As<ICommandDispatcher>().SingleInstance();
		}
	}
}