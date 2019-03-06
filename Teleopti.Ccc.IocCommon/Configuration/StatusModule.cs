using Autofac;
using Teleopti.Ccc.Domain.ETL;
using Teleopti.Ccc.Domain.Status;
using Teleopti.Ccc.Infrastructure.ETL;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class StatusModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TimeSinceLastEtlPing>().As<ITimeSinceLastEtlPing>().As<IMarkEtlPing>().SingleInstance();
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