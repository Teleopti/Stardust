﻿using System;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class ServiceBusCommonModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => StateHolderReader.Instance.StateReader.ApplicationScopeData).As<IApplicationData>().ExternallyOwned();
			builder.Register(c => UnitOfWorkFactoryContainer.Current).As<ICurrentUnitOfWorkFactory>().ExternallyOwned();
			builder.RegisterType<CurrentUnitOfWork>().As<ICurrentUnitOfWork>().SingleInstance();
			builder.RegisterType<CurrentDataSource>().As<ICurrentDataSource>().SingleInstance();
		
			builder.RegisterType<SendPushMessageWhenRootAlteredService>().As<ISendPushMessageWhenRootAlteredService>().InstancePerDependency();
			builder.RegisterType<RepositoryFactory>().As<IRepositoryFactory>().SingleInstance();
			builder.RegisterType<GroupingReadOnlyRepository>().As<IGroupingReadOnlyRepository>().SingleInstance();
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
			get => _current ?? UnitOfWorkFactory.CurrentUnitOfWorkFactory();
			set => _current = value;
		}
	}

}