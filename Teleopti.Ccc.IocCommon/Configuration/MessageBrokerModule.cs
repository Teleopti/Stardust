using System;
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
	public class MessageBrokerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(MessageFilterManager.Instance).As<IMessageFilterManager>().SingleInstance();

			builder.RegisterType<MessageBrokerCompositeClient>()
				.As<IMessageBrokerComposite>()
				.As<IMessageCreator>()
				.As<IMessageListener>()
				.SingleInstance();

			builder.Register(c => new RecreateOnNoPingReply(TimeSpan.FromMinutes(1))).As<IConnectionKeepAliveStrategy>();
			builder.Register(c => new RestartOnClosed(TimeSpan.Zero)).As<IConnectionKeepAliveStrategy>();
			builder.RegisterType<SignalRClient>()
				.As<ISignalRClient>()
				.As<IMessageBrokerUrl>()
				.WithParameter(new NamedParameter("serverUrl", null))
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