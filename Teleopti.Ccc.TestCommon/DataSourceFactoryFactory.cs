using System;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.TestCommon
{
	public class DataSourceFactoryFactory
	{
		private readonly Func<IDataSourcesFactory> _maker;

		public DataSourceFactoryFactory(Func<IDataSourcesFactory> maker)
		{
			_maker = maker;
		}

		public IDataSourcesFactory Make()
		{
			return _maker.Invoke();
		}
		
		public static DataSourceFactoryFactory MakeFromContainer(IComponentContext container)
		{
			return new DataSourceFactoryFactory(container.Resolve<IDataSourcesFactory>);
		}

		public static DataSourceFactoryFactory MakeLegacyWay(EnversConfiguration enversConfiguration = null)
		{
			return new DataSourceFactoryFactory(() =>
			{
				var mappingAssemblies = new[] {new DataSourceMappingAssembly {Assembly = typeof(Person).Assembly}, new DataSourceMappingAssembly {Assembly = typeof(Rta).Assembly}};
				enversConfiguration = enversConfiguration ?? new EnversConfiguration();
				return new DataSourcesFactory(
					enversConfiguration,
					new DataSourceConfigurationSetter(
						new DataSourceApplicationName {Name = DataSourceApplicationName.ForTest()},
						new ConfigReader(),
						mappingAssemblies),
					new MemoryNHibernateConfigurationCache(),
					new UnitOfWorkFactoryFactory(
						new NoPreCommitHooks(),
						null,
						new CurrentHttpContext(),
						ServiceLocator_DONTUSE.UpdatedBy,
						ServiceLocator_DONTUSE.CurrentBusinessUnit,
						new SirLeakAlot()),
					ServiceLocator_DONTUSE.UpdatedBy);
			});
		}
	}
}
