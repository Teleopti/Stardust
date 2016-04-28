using System.Configuration;
using System.Web.Http;
using Autofac;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class ContainerConfiguration : IContainerConfiguration
	{
		public IContainer Configure(string featureTogglePath, HttpConfiguration httpConfiguration)
		{
			var builder = new ContainerBuilder();

			var disableSecondLevelCache = ConfigurationManager.AppSettings["DisableSecondLevelCache"];
			var dataSourceConfigurationSetter = !string.IsNullOrEmpty(disableSecondLevelCache) &&
												disableSecondLevelCache.ToLowerInvariant() == "true"
				? DataSourceConfigurationSetter.ForLoadBalancedWeb()
				: DataSourceConfigurationSetter.ForWeb();

			var args = new IocArgs(new ConfigReader())
			{
				FeatureToggle = featureTogglePath,
				DataSourceConfigurationSetter = dataSourceConfigurationSetter
			};
			var configuration = new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args));
			builder.RegisterModule(new WebAppModule(configuration, httpConfiguration));

			return builder.Build();
		}
	}
}
