using System.Configuration;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.SystemCheck;
using Teleopti.Ccc.Infrastructure.SystemCheck;
using Teleopti.Interfaces.Domain;
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
			builder.RegisterType<NotificationCreator>().As<INotificationCreator>().SingleInstance();
			
			builder.RegisterType<MessageBrokerCompositeClient>()
				.As<IMessageBrokerComposite>()
				.As<IMessageCreator>()
				.As<IMessageListener>()
				.SingleInstance();

			var signalRRequired = _configuration.Args().MessageBrokerListeningEnabled;
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
			else
			{
				builder.RegisterType<DisabledSignalRClient>()
					.As<ISignalRClient>()
					.As<IMessageBrokerUrl>()
					.SingleInstance();
			}

			builder.RegisterType<HttpClientM>().SingleInstance();
			builder.RegisterType<HttpSender>().As<IMessageSender>().SingleInstance();
			builder.RegisterType<HttpServer>().As<IHttpServer>().SingleInstance();

			builder.RegisterType<SystemCheckerValidator>();
			builder.RegisterType<CheckMessageBroker>().As<ISystemCheck>();
			if (_configuration.Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733))
				builder.RegisterType<CheckMessageBrokerMailBox>().As<ISystemCheck>();
		}
	}
}