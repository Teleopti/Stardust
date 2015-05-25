using Autofac;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class MessageBrokerServerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SubscriptionFiller>().As<IBeforeSubscribe>().SingleInstance();
			builder.RegisterType<MessageBrokerServer>().As<IMessageBrokerServer>().SingleInstance();
			builder.RegisterType<EmtpyMailboxRepository>()
				.As<IMailboxRepository>()
				.SingleInstance()
				;
		}
	}
}