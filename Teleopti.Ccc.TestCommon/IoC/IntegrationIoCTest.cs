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

		public static void Setup(Action<ContainerBuilder> registrations)
		{
			Setup(registrations, null, null);
		}
		
		public static void Setup(Action<ContainerBuilder> registrations, Action<IocArgs> arguments)
		{
			Setup(registrations, arguments, null);
		}

		public static void Setup(Action<ContainerBuilder> registrations, Action<IocArgs> arguments, object injectTo)
		{
			var builder = new ContainerBuilder();
			var iocArgs = new IocArgs(new ConfigReader())
			{
				FeatureToggle = TestSiteConfigurationSetup.URL.ToString()
			};
			arguments?.Invoke(iocArgs);
			var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));

			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterType<TenantAuthenticationAlwaysAuthenticated>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterModule(new DataFactoryModule());

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