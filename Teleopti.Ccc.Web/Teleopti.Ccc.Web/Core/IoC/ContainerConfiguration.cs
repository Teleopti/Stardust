using System.Collections.Generic;
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

			var args = new IocArgs(new ConfigOverrider(new ConfigReader(),new Dictionary<string, string> { {"FCM", "key=AAAANvMkWNA:APA91bG1pR8ZVsp-S98uWsFUE5lnQiC8UnsQL3DgN6Vyw5HyaKuqVt86kdeurfLfQkWt_7kZTgXcTuAaxvcVUkjtE8jFo72loTy6UYrLrVbYnqCXVI4mWCYhvLQnU3Sv0sIfW1k-eZCu" } }))
			{
				FeatureToggle = featureTogglePath,
				DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForWeb(),
				CacheRulesetPerLifeTimeScope = false
			};
			var configuration = new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args));
			builder.RegisterModule(new WebAppModule(configuration, httpConfiguration));

			return builder.Build();
		}
	}
}
