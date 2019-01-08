using System.Configuration;
using System.Net;
using System.Net.Http;
using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.SystemCheck;
using Teleopti.Ccc.Infrastructure.SystemCheck;
using Teleopti.Messaging.Client.Composite;
using Teleopti.Messaging.Client.Http;
using Teleopti.Messaging.Client.SignalR;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class MessageBrokerModule : Module
	{
		private readonly IocConfiguration _config;

		public MessageBrokerModule(IocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(MessageFilterManager.Instance).As<IMessageFilterManager>().SingleInstance();
			builder.RegisterType<NotificationCreator>().As<INotificationCreator>().SingleInstance();

			builder.RegisterType<MessageBrokerCompositeClient>()
				.As<IMessageBrokerComposite>()
				.As<IMessageCreator>()
				.As<IMessageListener>()
				.SingleInstance();

			var signalRRequired = _config.Args().MessageBrokerListeningEnabled;
			if (_config.Args().SharedContainer != null)
			{
				builder.RegisterInstance(_config.Args().SharedContainer.Resolve<ISignalRClient>())
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
			else
			{
				builder.RegisterType<DisabledSignalRClient>()
					.As<ISignalRClient>()
					.As<IMessageBrokerUrl>()
					.SingleInstance();
			}

			builder.RegisterType<HttpClientM>().SingleInstance();
			builder.Register(c => new HttpClient(
				new HttpClientHandler
				{
					Credentials = CredentialCache.DefaultNetworkCredentials
				})).SingleInstance().ExternallyOwned();
			if (_config.IsToggleEnabled(Toggles.MessageBroker_HttpSenderThrottleRequests_79140))
				builder.RegisterType<HttpThrottledSender>().As<IMessageSender>().SingleInstance();
			else
				builder.RegisterType<HttpSender>().As<IMessageSender>().SingleInstance();
			builder.RegisterType<HttpServer>().As<IHttpServer>().SingleInstance();

			builder.RegisterType<SystemCheckerValidator>();
			builder.RegisterType<CheckMessageBroker>().As<ISystemCheck>();
			builder.RegisterType<CheckMessageBrokerMailBox>().As<ISystemCheck>();
		}
	}
}