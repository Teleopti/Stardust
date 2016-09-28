using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.Aop;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class AspectsModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AspectInterceptor>().SingleInstance();
			builder.RegisterType<LogInfoAspect>().SingleInstance();
			builder.RegisterType<LogManagerWrapper>().As<ILogManagerWrapper>().SingleInstance();
			builder.RegisterType<LogTimeAspect>().InstancePerDependency();
		}
	}
}