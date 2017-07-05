using Autofac;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ServiceBusModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SignatureCreator>().SingleInstance();
		}
	}
}