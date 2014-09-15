using System.Configuration;
using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Client.Composite;
using Teleopti.Messaging.Client.Http;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class MessageBrokerModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public MessageBrokerModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(MessageFilterManager.Instance).As<IMessageFilterManager>().SingleInstance();

			builder.RegisterType<MessageBrokerCompositeClient>()
				.As<IMessageBrokerComposite>()
				.As<IMessageCreator>()
				.As<IMessageListener>()
				.SingleInstance();

			builder.Register(c =>
			{
				if (_configuration.Args().MessageBrokerListeningEnabled)
					return (ISignalRClient) _configuration.Args().ResolveSharedComponent<SignalRClient>(c);
				if (_configuration.Toggle(Toggles.Messaging_HttpSender_29205))
					return _configuration.Args().ResolveSharedComponent<DisabledSignalRClient>(c);
				return _configuration.Args().ResolveSharedComponent<SignalRClient>(c);
			})
				.As<ISignalRClient>()
				.As<IMessageBrokerUrl>()
				.SingleInstance();

			if (_configuration.Args().MessageBrokerListeningEnabled)
			{
				builder.RegisterType<RecreateOnNoPingReply>().As<IConnectionKeepAliveStrategy>().SingleInstance();
				builder.RegisterType<RestartOnClosed>().As<IConnectionKeepAliveStrategy>().SingleInstance();
			}
			else
				builder.RegisterType<DisabledSignalRClient>().SingleInstance();

			builder.RegisterType<SignalRClient>()
				.WithParameter(new NamedParameter("serverUrl", ConfigurationManager.AppSettings["MessageBroker"]))
				.SingleInstance();

			builder.RegisterType<HttpSender>()
				.As<HttpSender>()
				.SingleInstance();

			builder.RegisterType<SignalRSender>()
				.As<SignalRSender>()
				.SingleInstance();

			builder.Register(c => _configuration.Toggle(Toggles.Messaging_HttpSender_29205)
				? c.Resolve<HttpSender>() : (IMessageSender)c.Resolve<SignalRSender>())
				.As<IMessageSender>()
				.SingleInstance();
		}
	}

}