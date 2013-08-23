using Autofac;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class InitializeModule : Module
	{
		private readonly IDataSourceConfigurationSetter _dataSourceConfigurationSetter;

		public InitializeModule(IDataSourceConfigurationSetter dataSourceConfigurationSetter)
		{
			_dataSourceConfigurationSetter = dataSourceConfigurationSetter;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(_dataSourceConfigurationSetter);
			builder.RegisterType<InitializeApplication>().As<IInitializeApplication>().SingleInstance();
			builder.RegisterType<DataSourcesFactory>().As<IDataSourcesFactory>().SingleInstance();
			builder.RegisterInstance(new SignalBroker(MessageFilterManager.Instance))
				.As<IMessageBroker>()
				.As<IMessageBrokerSender>()
				.As<IMessageBrokerListener>()
				;
			builder.RegisterType<OneWayEncryption>().As<IOneWayEncryption>().SingleInstance();
			builder.RegisterType<EnversConfiguration>().As<IEnversConfiguration>().SingleInstance();
			builder.RegisterType<ConfigReader>().As<IConfigReader>().SingleInstance();
			builder.RegisterType<ConfigurationManagerWrapper>().As<IConfigurationWrapper>().SingleInstance();
		}
	}
}