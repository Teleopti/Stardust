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

			var signalRRequired =
				_configuration.Args().MessageBrokerListeningEnabled ||
				!_configuration.Toggle(Toggles.Messaging_HttpSender_29205);

			if (_configuration.Args().SharedContainer != null)
			{
				builder.RegisterInstance(_configuration.Args().SharedContainer.Resolve<ISignalRClient>())
					.As<ISignalRClient>()
					.As<IMessageBrokerUrl>()
					.SingleInstance();
			} 
			else if (signalRRequired)
			{
				builder.RegisterType<RecreateOnNoPingReply>().As<IConnectionKeepAliveStrategy>().SingleInstance();
				builder.RegisterType<RestartOnClosed>().As<IConnectionKeepAliveStrategy>().SingleInstance();
				builder.RegisterType<SignalRClient>()
					.WithParameter(new NamedParameter("serverUrl", ConfigurationManager.AppSettings["MessageBroker"]))
					.As<ISignalRClient>()
					.As<IMessageBrokerUrl>()
					.SingleInstance();
			} 
			else if (_configuration.Toggle(Toggles.Messaging_HttpSender_29205))
			{
				builder.RegisterType<DisabledSignalRClient>()
					.As<ISignalRClient>()
					.As<IMessageBrokerUrl>()
					.SingleInstance();
			}

			if (_configuration.Toggle(Toggles.Messaging_HttpSender_29205))
				builder.RegisterType<HttpSender>().As<IMessageSender>().SingleInstance();
			else
				builder.RegisterType<SignalRSender>().As<IMessageSender>().SingleInstance();

		}
	}

}