using System;
using Autofac;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class IntegrationIoCTest
	{
		public static IContainer Container { get; private set; }

		public static void Setup()
		{
			Setup(null, null);
		}

		public static void Setup(Action<ContainerBuilder> registrations, object injectTo)
		{
			var builder = new ContainerBuilder();
			var args = new IocArgs(new ConfigReader())
			{
				AllEventPublishingsAsSync = true,
				FeatureToggle = TestSiteConfigurationSetup.URL.ToString()
			};
			var configuration = new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args));

			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterModule(new TenantServerModule(configuration));
			builder.RegisterType<TenantAuthenticationAlwaysAuthenticated>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterModule(new TestModule());

			registrations?.Invoke(builder);

			Container = builder.Build();

			if (injectTo != null)
				IoCTestService.InjectTo(Container, injectTo);
		}

		public static void Dispose()
		{
			Container?.Dispose();
			Container = null;
		}
	}
}