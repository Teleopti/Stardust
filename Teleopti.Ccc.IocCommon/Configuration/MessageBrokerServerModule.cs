using System;
using System.Collections.Generic;
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
			builder.RegisterType<MailboxRepository>().As<IMailboxRepository>().SingleInstance();
		}
	}

	public class MailboxRepository : IMailboxRepository
	{
		public void Persist(Mailbox mailbox)
		{
		}

		public Mailbox Get(Guid id)
		{
			return null;
		}

		public IEnumerable<Mailbox> Get(string route)
		{
			return new Mailbox[] {};
		}
	}
}