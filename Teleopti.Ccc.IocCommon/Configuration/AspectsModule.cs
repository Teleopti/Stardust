using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Infrastructure.Aop;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class AspectsModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AspectInterceptor>();
			builder.RegisterType<InfoLogAspect>().As<ILogAspect>();
			builder.RegisterType<LogManagerWrapper>().As<ILogManagerWrapper>();
			builder.RegisterType<LogTimeAspect>().InstancePerDependency();
		}
	}
}