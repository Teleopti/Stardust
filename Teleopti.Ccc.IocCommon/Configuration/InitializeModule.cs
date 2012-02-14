using Autofac;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Composites;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class InitializeModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<InitializeApplication>().As<IInitializeApplication>().SingleInstance();
			builder.RegisterType<DataSourcesFactory>().As<IDataSourcesFactory>().SingleInstance();
			builder.RegisterInstance(MessageBrokerImplementation.GetInstance(MessageFilterManager.Instance.FilterDictionary));
			builder.RegisterType<OneWayEncryption>().As<IOneWayEncryption>().SingleInstance();
			builder.RegisterType<EnversConfiguration>().As<IEnversConfiguration>().SingleInstance();
		}
	}
}