using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.MessageBroker;

namespace Teleopti.Ccc.IocCommon
{
	public class MailboxModule : Module
	{
		//private readonly IIocConfiguration _configuration;

		//public MailboxModule(IIocConfiguration configuration)
		//{
		//	_configuration = configuration;
		//}

		protected override void Load(ContainerBuilder builder)
		{
			//if (_configuration.Toggle(Toggles.MessageBroker_Mailbox_32733))
			//	builder.RegisterType<MailboxRepository>()
			//		.As<IMailboxRepository>()
			//		.SingleInstance();
			//else
				builder.RegisterType<NoMailboxRepository>()
					.As<IMailboxRepository>()
					.SingleInstance();
		}
	}
}