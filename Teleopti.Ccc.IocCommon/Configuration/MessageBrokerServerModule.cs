using Autofac;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MessageBroker;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class MessageBrokerServerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SubscriptionFiller>().As<IBeforeSubscribe>().SingleInstance();
			builder.RegisterType<MessageBrokerServer>().As<IMessageBrokerServer>().SingleInstance();
			builder.RegisterType<MailboxRepository>()
				.As<IMailboxRepository>()
				.SingleInstance()
				.ApplyAspects()
				;
		}
	}
}