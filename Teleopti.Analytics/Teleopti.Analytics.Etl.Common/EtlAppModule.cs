using Autofac;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Analytics.Etl.Common
{
	public class EtlAppModule : Module
	{
		private IConfigReader _configReader = new ConfigReader();

		public void SetConfigReader(IConfigReader configReader)
		{
			_configReader = configReader;
		}

		protected override void Load(ContainerBuilder builder)
		{
			var iocArgs = new IocArgs(_configReader)
			{
				DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForEtl()
			};
			var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterModule(new EtlModule(configuration));
		}
	}
}