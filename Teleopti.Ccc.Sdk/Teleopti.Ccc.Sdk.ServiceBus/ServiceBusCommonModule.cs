using System;
using Autofac;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class ServiceBusCommonModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<BusStartup>().As<IServiceBusAware>().SingleInstance();
			builder.Register(c => StateHolderReader.Instance.StateReader.ApplicationScopeData).As<IApplicationData>().ExternallyOwned();
			builder.Register(c => UnitOfWorkFactoryContainer.Current).As<ICurrentUnitOfWorkFactory>().ExternallyOwned();
			builder.RegisterType<CurrentUnitOfWork>().As<ICurrentUnitOfWork>().SingleInstance();
			builder.RegisterType<CurrentDataSource>().As<ICurrentDataSource>().SingleInstance();
			
			builder.RegisterType<SendPushMessageWhenRootAlteredService>().As<ISendPushMessageWhenRootAlteredService>().InstancePerDependency();
			builder.RegisterType<RepositoryFactory>().As<IRepositoryFactory>().SingleInstance();
			builder.RegisterType<InternalServiceBusSender>().As<IServiceBusSender>().SingleInstance();
			builder.RegisterType<GroupingReadOnlyRepository>().As<IGroupingReadOnlyRepository>().SingleInstance();
			registerDataSourcesFactoryDependencies(builder);
		}

		private static void registerDataSourcesFactoryDependencies(ContainerBuilder builder)
		{
			builder.RegisterType<InternalServiceBusSender>().As<IServiceBusSender>().SingleInstance();
			builder.RegisterType<SetNoLicenseActivator>().As<ISetLicenseActivator>().SingleInstance();
			builder.Register(c => DataSourceConfigurationSetter.ForServiceBus())
				.As<IDataSourceConfigurationSetter>()
				.SingleInstance();
		}

		
	}
}