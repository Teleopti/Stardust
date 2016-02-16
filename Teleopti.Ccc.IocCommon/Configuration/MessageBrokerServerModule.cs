using Autofac;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MessageBroker;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class MessageBrokerServerModule : Module
	{
		private readonly bool _throttleMessages;
		private readonly int _messagesPerSecond;

		public MessageBrokerServerModule(IIocConfiguration configuration)
		{
			_throttleMessages = configuration.Args().ThrottleMessages;
			_messagesPerSecond = configuration.Args().MessagesPerSecond;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SubscriptionFiller>().As<IBeforeSubscribe>().SingleInstance();
			builder.RegisterType<MessageBrokerServer>().As<IMessageBrokerServer>().SingleInstance().ApplyAspects();
			builder.RegisterType<MailboxRepository>().As<IMailboxRepository>().SingleInstance();

			if (_throttleMessages)
			{
				var actionThrottle = new ActionThrottle(_messagesPerSecond);
				actionThrottle.Start();
				builder.RegisterInstance(actionThrottle).As<IActionScheduler>().SingleInstance();
			}
			else
			{
				builder.RegisterType<ActionImmediate>().As<IActionScheduler>().SingleInstance();
			}
		}
	}
	
}