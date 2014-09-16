using System;
using System.Configuration;
using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Toggle;
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
		public bool MessageBrokerListeningEnabled { get; set; }
		public Func<IComponentContext, SignalRClient> SharedSignalRClient { get; set; }

		protected override void Load(ContainerBuilder builder)
		{
			var resolveSignalRClient = SharedSignalRClient ?? (c => c.Resolve<SignalRClient>());

			builder.RegisterInstance(MessageFilterManager.Instance).As<IMessageFilterManager>().SingleInstance();

			builder.RegisterType<MessageBrokerCompositeClient>()
				.As<IMessageBrokerComposite>()
				.As<IMessageCreator>()
				.As<IMessageListener>()
				.SingleInstance();

			builder.Register(c =>
			{
				if (MessageBrokerListeningEnabled)
					return (ISignalRClient) resolveSignalRClient(c);
				if (c.Resolve<IToggleManager>().IsEnabled(Toggles.Messaging_HttpSender_29205))
					return c.Resolve<DisabledSignalRClient>();
				return resolveSignalRClient(c);
			})
				.As<ISignalRClient>()
				.As<IMessageBrokerUrl>()
				.SingleInstance();

			if (MessageBrokerListeningEnabled)
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

			builder.Register(c => c.Resolve<IToggleManager>().IsEnabled(Toggles.Messaging_HttpSender_29205)
				? c.Resolve<HttpSender>() : (IMessageSender)c.Resolve<SignalRSender>())
				.As<IMessageSender>()
				.SingleInstance();
		}
	}
}