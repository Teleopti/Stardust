using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MessageBroker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class MessageBrokerServerModule : Module
	{
		private readonly bool _throttleMessages;
		private readonly int _messagesPerSecond;

		public MessageBrokerServerModule(IIocConfiguration configuration)
		{
			_throttleMessages = configuration.Args().ThrottleMessages;
			_messagesPerSecond = configuration.Args().MessagesPerSecond;
		}

		public MessageBrokerServerModule(bool throttleMessages, int messagesPerSecond)
		{
			_throttleMessages = throttleMessages;
			_messagesPerSecond = messagesPerSecond;
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
				builder.Register(c => actionThrottle).As<IActionScheduler>().ExternallyOwned();
			}
			else
			{
				builder.RegisterType<ActionImmediate>().As<IActionScheduler>().SingleInstance();
			}
		}
	}
	
}