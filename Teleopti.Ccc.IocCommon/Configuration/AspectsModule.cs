using Autofac;
using Teleopti.Ccc.Infrastructure.Aop;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class AspectsModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AspectInterceptor>();
		}
	}
}