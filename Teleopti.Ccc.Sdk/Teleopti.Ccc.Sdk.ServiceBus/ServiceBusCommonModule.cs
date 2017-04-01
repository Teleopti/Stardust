using System;
using Autofac;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using DeviceInfoProvider = Teleopti.Ccc.Sdk.ServiceBus.HealthCheck.DeviceInfoProvider;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

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
		
			builder.RegisterType<InternalServiceBusSender>().As<IServiceBusSender>().SingleInstance();
			builder.RegisterType<ServiceBusDelayedMessageSender>().As<IDelayedMessageSender>().SingleInstance();

			builder.RegisterType<SendPushMessageWhenRootAlteredService>().As<ISendPushMessageWhenRootAlteredService>().InstancePerDependency();
			builder.RegisterType<RepositoryFactory>().As<IRepositoryFactory>().SingleInstance();
			builder.RegisterType<GroupingReadOnlyRepository>().As<IGroupingReadOnlyRepository>().SingleInstance();
			builder.RegisterType<DeviceInfoProvider>().As<IDeviceInfoProvider>().SingleInstance();
			registerDataSourcesFactoryDependencies(builder);
		}

		private static void registerDataSourcesFactoryDependencies(ContainerBuilder builder)
		{
			builder.RegisterType<SetNoLicenseActivator>().As<ISetLicenseActivator>().SingleInstance();
			builder.Register(c => DataSourceConfigurationSetter.ForServiceBus())
				.As<IDataSourceConfigurationSetter>()
				.SingleInstance();
		}

	}

	public static class UnitOfWorkFactoryContainer
	{
		[ThreadStatic]
		private static ICurrentUnitOfWorkFactory _current;

		public static ICurrentUnitOfWorkFactory Current
		{
			get { return _current ?? UnitOfWorkFactory.CurrentUnitOfWorkFactory(); }
			set { _current = value; }
		}
	}

}