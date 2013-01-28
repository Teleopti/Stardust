using Autofac;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class AuthenticationContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ClaimCache>().SingleInstance();
		}
    }
}