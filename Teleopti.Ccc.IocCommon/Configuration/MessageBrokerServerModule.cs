using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.MessageBroker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class MessageBrokerServerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SubscriptionFiller>().As<IBeforeSubscribe>().SingleInstance();
			builder.RegisterType<MessageBrokerServer>().As<IMessageBrokerServer>().SingleInstance();
			builder.RegisterType<EmtpyMailboxRepository>().As<IMailboxRepository>().SingleInstance();
			//builder.RegisterType<MailboxRepository>().As<IMailboxRepository>().SingleInstance();
		}
	}

	public class EmtpyMailboxRepository : IMailboxRepository
	{
		public void Persist(Mailbox mailbox)
		{
		}

		public Mailbox Load(Guid id)
		{
			return new Mailbox();
		}

		public IEnumerable<Mailbox> Load(string[] routes)
		{
			return Enumerable.Empty<Mailbox>();
		}
	}
}