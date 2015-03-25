using System.Web.Http;
using Autofac;
using MbCache.Configuration;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.MultipleConfig;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class ContainerConfiguration : IContainerConfiguration
	{
		public IContainer Configure(string featureTogglePath, HttpConfiguration httpConfiguration)
		{
			var builder = new ContainerBuilder();

			var args = new IocArgs(new AppConfigReader())
			{
				FeatureToggle = featureTogglePath,
				CacheLockObjectGenerator = new FixedNumberOfLockObjects(100),
				DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForWeb()
			};
			var configuration = new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args));
			builder.RegisterModule(new WebAppModule(configuration, httpConfiguration));

			return builder.Build();
		}
	}
}
