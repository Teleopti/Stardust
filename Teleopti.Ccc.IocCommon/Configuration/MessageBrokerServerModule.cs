﻿using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MessageBroker;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class MessageBrokerServerModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public MessageBrokerServerModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SubscriptionFiller>().As<IBeforeSubscribe>().SingleInstance();
			builder.RegisterType<MessageBrokerServer>().As<IMessageBrokerServer>().SingleInstance().ApplyAspects();
			//if (_configuration.Toggle(Toggles.Mailbox_Optimization_41900))
			//	builder.RegisterType<MailboxRepository2>().As<IMailboxRepository>().SingleInstance();
			//else
				builder.RegisterType<MailboxRepository>().As<IMailboxRepository>().SingleInstance();
		}
	}
	
}