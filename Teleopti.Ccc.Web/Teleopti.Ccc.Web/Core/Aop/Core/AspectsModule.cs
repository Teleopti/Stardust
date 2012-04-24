using Autofac;

namespace Teleopti.Ccc.Web.Core.Aop.Core
{
	public class AspectsModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AspectInterceptor>();
		}
	}
}