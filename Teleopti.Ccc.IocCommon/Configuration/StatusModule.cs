using Autofac;
using Teleopti.Ccc.Domain.Status;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class StatusModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CheckLegacySystemStatus>().SingleInstance();
			builder.RegisterType<ExecuteStatusStep>().SingleInstance();
			builder.RegisterType<ListStatusSteps>().SingleInstance();
			builder.RegisterType<CallLegacySystemStatus>().As<ICallLegacySystemStatus>();
			builder.RegisterAssemblyTypes(typeof(IStatusStep).Assembly)
				.Where(t => typeof(IStatusStep).IsAssignableFrom(t))
				.As<IStatusStep>()
				.AsSelf()
				.SingleInstance();
		}
	}
}