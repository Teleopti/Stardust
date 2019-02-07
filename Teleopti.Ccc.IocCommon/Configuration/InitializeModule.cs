using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class InitializeModule : Module
	{
		private readonly IocConfiguration _iocConfiguration;

		public InitializeModule(IocConfiguration iocConfiguration)
		{
			_iocConfiguration = iocConfiguration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(new DataSourceApplicationName {Name = _iocConfiguration.Args().DataSourceApplicationName});
			builder.RegisterInstance(new DataSourceMappingAssembly {Assembly = typeof(Person).Assembly});
			builder.RegisterInstance(new DataSourceMappingAssembly {Assembly = typeof(Rta).Assembly});
			builder.RegisterType<DataSourceConfigurationSetter>().As<IDataSourceConfigurationSetter>().SingleInstance();
			builder.RegisterType<InitializeApplication>().As<IInitializeApplication>().SingleInstance();
			builder.RegisterType<MemoryNHibernateConfigurationCache>().SingleInstance();
			builder.RegisterType<UnitOfWorkFactoryFactory>().SingleInstance();
			builder.RegisterType<DataSourcesFactory>().As<IDataSourcesFactory>().SingleInstance();
			builder.RegisterType<InitializeLicenseServiceForTenant>().As<IInitializeLicenseServiceForTenant>().SingleInstance();
			builder.RegisterType<LicenseVerifierFactory>().As<ILicenseVerifierFactory>().SingleInstance();
			builder.RegisterType<LicenseRepositoryForLicenseVerifier>().As<ILicenseRepositoryForLicenseVerifier>().SingleInstance();
			builder.RegisterType<EnversConfiguration>().SingleInstance();
		}
	}
}