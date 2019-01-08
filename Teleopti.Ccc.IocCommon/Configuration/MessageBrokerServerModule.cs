﻿using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MessageBroker;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class MessageBrokerServerModule : Module
	{
		private readonly IocConfiguration _config;

		public MessageBrokerServerModule(IocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SubscriptionFiller>().As<IBeforeSubscribe>().SingleInstance();
			if (_config.IsToggleEnabled(Toggles.MessageBroker_ServerThrottleMessages_79140))
				builder.RegisterType<MessageBrokerServerThrottleMessages>().As<IMessageBrokerServer>().SingleInstance().ApplyAspects();	
			else if (_config.IsToggleEnabled(Toggles.MessageBroker_VeganBurger_79140))
				builder.RegisterType<MessageBrokerServerNoMailboxPurge>().As<IMessageBrokerServer>().SingleInstance().ApplyAspects();
			else
				builder.RegisterType<MessageBrokerServer>().As<IMessageBrokerServer>().SingleInstance().ApplyAspects();
			builder.RegisterType<MailboxRepository>().As<IMailboxRepository>().SingleInstance();
		}
	}
}