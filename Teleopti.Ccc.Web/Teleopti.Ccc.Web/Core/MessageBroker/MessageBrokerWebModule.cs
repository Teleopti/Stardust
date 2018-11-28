using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerWebModule : Module
	{
		private readonly IocConfiguration _config;

		public MessageBrokerWebModule(IocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SignalR>().As<ISignalR>().SingleInstance();

			if (_config.Toggle(Toggles.MessageBroker_ServerThrottleMessages_79140))
				builder.RegisterType<InProcessMessageSenderNoThrottle>().As<IMessageSender>().SingleInstance();
			else
				builder.RegisterType<InProcessMessageSender>().As<IMessageSender>().SingleInstance();
		}
	}
}