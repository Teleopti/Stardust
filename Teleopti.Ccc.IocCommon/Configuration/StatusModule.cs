using Autofac;
using Teleopti.Ccc.Domain.ETL;
using Teleopti.Ccc.Domain.Status;
using Teleopti.Ccc.Infrastructure.ETL;
using Teleopti.Ccc.Infrastructure.Status;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class StatusModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PingCustomStep>().SingleInstance();
			builder.RegisterType<StoreNewCustomStatusStep>().SingleInstance();
			builder.RegisterType<AllSteps>().SingleInstance();
			builder.RegisterType<FetchCustomStatusSteps>().As<IFetchCustomStatusSteps>().SingleInstance();
			builder.RegisterType<TimeSinceLastEtlPing>().As<ITimeSinceLastEtlPing>().As<IMarkEtlPing>().SingleInstance();
			builder.RegisterType<CheckLegacySystemStatus>().SingleInstance();
			builder.RegisterType<ExecuteStatusStep>().SingleInstance();
			builder.RegisterType<ListStatusSteps>().SingleInstance();
			builder.RegisterType<CallLegacySystemStatus>().As<ICallLegacySystemStatus>();
			builder.RegisterAssemblyTypes(typeof(IStatusStep).Assembly)
				.Where(t => typeof(IStatusStep).IsAssignableFrom(t) && t != typeof(CustomStatusStep))
				.As<IStatusStep>()
				.AsSelf()
				.SingleInstance();
		}
	}
}