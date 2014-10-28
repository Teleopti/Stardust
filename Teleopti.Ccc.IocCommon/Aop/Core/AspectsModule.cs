using Autofac;

namespace Teleopti.Ccc.IocCommon.Aop.Core
{
	public class AspectsModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AspectInterceptor>();
		}
	}
}