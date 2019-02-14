using Autofac;
using Teleopti.Ccc.Domain.MonitorSystem;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class MonitorModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CheckLegacySystemStatus>().SingleInstance();
			builder.RegisterType<CallLegacySystemStatus>().As<ICallLegacySystemStatus>();
		}
	}
}