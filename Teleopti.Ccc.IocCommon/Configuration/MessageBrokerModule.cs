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
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(MessageFilterManager.Instance).As<IMessageFilterManager>().SingleInstance();

			builder.RegisterType<MessageBrokerCompositeClient>()
				.As<IMessageBrokerComposite>()
				.As<IMessageCreator>()
				.As<IMessageListener>()
				.SingleInstance();

			builder.RegisterType<RecreateOnNoPingReply>().As<IConnectionKeepAliveStrategy>();
			builder.RegisterType<RestartOnClosed>().As<IConnectionKeepAliveStrategy>();
			builder.RegisterType<SignalRClient>()
				.As<ISignalRClient>()
				.As<IMessageBrokerUrl>()
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