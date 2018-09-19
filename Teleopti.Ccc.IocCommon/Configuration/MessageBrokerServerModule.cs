using Autofac;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MessageBroker;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class MessageBrokerServerModule : Module
	{
		private readonly IocConfiguration _configuration;

		public MessageBrokerServerModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SubscriptionFiller>().As<IBeforeSubscribe>().SingleInstance();
			builder.RegisterType<MessageBrokerServer>().As<IMessageBrokerServer>().SingleInstance().ApplyAspects();
			builder.RegisterType<MailboxRepository>().As<IMailboxRepository>().SingleInstance();
		}
	}
	
}