﻿using System.Web.Http;
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

			var args = new IocArgs(new ConfigReader())
			{
				FeatureToggle = featureTogglePath,
				DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForWeb()
			};
			var configuration = new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args));
			builder.RegisterModule(new WebAppModule(configuration, httpConfiguration));

			return builder.Build();
		}
	}
}
