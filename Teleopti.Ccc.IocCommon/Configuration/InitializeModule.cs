using Autofac;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class InitializeModule : Module
	{
		private readonly IIocConfiguration _iocConfiguration;

		public InitializeModule(IIocConfiguration iocConfiguration)
		{
			_iocConfiguration = iocConfiguration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(_iocConfiguration.Args().DataSourceConfigurationSetter);
			builder.RegisterType<InitializeApplication>().As<IInitializeApplication>().SingleInstance();
			builder.RegisterType<MemoryNhibernateConfigurationCache>().As<INHibernateConfigurationCache>().SingleInstance();
			builder.RegisterType<DataSourcesFactory>().As<IDataSourcesFactory>().SingleInstance();
			builder.RegisterType<SetLicenseActivator>().As<ISetLicenseActivator>().SingleInstance();
			builder.RegisterType<LicenseVerifierFactory>().As<ILicenseVerifierFactory>().SingleInstance();
			builder.RegisterType<LicenseRepositoryForLicenseVerifier>().As<ILicenseRepositoryForLicenseVerifier>().SingleInstance();
			builder.RegisterType<EnversConfiguration>().As<IEnversConfiguration>().SingleInstance();
			builder.RegisterType<ConfigReader>().As<IConfigReader>().SingleInstance();
			if (_iocConfiguration.Toggle(Toggles.ResourcePlanner_LessPersonAssignmentUpdates_42159))
			{
				builder.RegisterType<UpdatePersonAssignmentLayersCollectionType>().As<IChangeNHibernateConfiguration>().SingleInstance();
			}
			else
			{
				builder.RegisterType<NoNHibernateConfigurationChange>().As<IChangeNHibernateConfiguration>().SingleInstance();
			}
		}
	}
}