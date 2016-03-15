using Autofac;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ServiceBusModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<MessagePopulatingServiceBusSender>().As<IMessagePopulatingServiceBusSender>().SingleInstance();
			builder.RegisterType<NoServiceBusSender>().As<IServiceBusSender>().SingleInstance();
			builder.RegisterType<SignatureCreator>().SingleInstance();
		}
	}
}