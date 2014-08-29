using Autofac;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Client.Composite;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class InitializeModule : Module
	{
		private readonly IDataSourceConfigurationSetter _dataSourceConfigurationSetter;

		public InitializeModule(IDataSourceConfigurationSetter dataSourceConfigurationSetter)
		{
			_dataSourceConfigurationSetter = dataSourceConfigurationSetter;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(_dataSourceConfigurationSetter);
			builder.RegisterType<InitializeApplication>().As<IInitializeApplication>().SingleInstance();
			builder.RegisterType<DataSourcesFactory>().As<IDataSourcesFactory>().SingleInstance();

			registerMessageBroker(builder);

			builder.RegisterType<OneWayEncryption>().As<IOneWayEncryption>().SingleInstance();
			builder.RegisterType<EnversConfiguration>().As<IEnversConfiguration>().SingleInstance();
			builder.RegisterType<ConfigReader>().As<IConfigReader>().SingleInstance();
			builder.RegisterType<ConfigurationManagerWrapper>().As<IConfigurationWrapper>().SingleInstance();
		}

		private static void registerMessageBroker(ContainerBuilder builder)
		{
			builder.RegisterInstance(MessageFilterManager.Instance).As<IMessageFilterManager>().SingleInstance();
			builder.RegisterType<RecreateOnNoPingReply>().As<IConnectionKeepAliveStrategy>();
			builder.RegisterType<RestartOnClosed>().As<IConnectionKeepAliveStrategy>();

			builder.RegisterType<SignalRClient>()
				.As<ISignalRClient>()
				.WithParameter(new NamedParameter("serverUrl", null))
				.SingleInstance();
			builder.RegisterType<SignalRSender>()
				.As<Interfaces.MessageBroker.Client.IMessageSender>()
				.SingleInstance();

			builder.RegisterType<SignalBroker>()
				.As<IMessageBroker>()
				.As<IMessageBrokerSender>()
				.As<IMessageBrokerListener>()
				.SingleInstance();
		}
	}
}