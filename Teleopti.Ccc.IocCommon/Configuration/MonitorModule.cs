using Autofac;
using Teleopti.Ccc.Domain.MonitorSystem;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class MonitorModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CheckLegacySystemStatus>().SingleInstance();
			builder.RegisterType<ExecuteMonitorStep>().SingleInstance();
			builder.RegisterType<ListMonitorSteps>().SingleInstance();
			builder.RegisterType<CallLegacySystemStatus>().As<ICallLegacySystemStatus>();
			builder.RegisterAssemblyTypes(typeof(IMonitorStep).Assembly)
				.Where(t => typeof(IMonitorStep).IsAssignableFrom(t))
				.As<IMonitorStep>()
				.AsSelf()
				.SingleInstance();
		}
	}
}