using Autofac;

namespace Teleopti.Ccc.IocCommon.Aop.Core
{
	internal class AspectsModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AspectInterceptor>();
		}
	}
}