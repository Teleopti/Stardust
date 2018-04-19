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

		public static void Setup(Action<ContainerBuilder> registrations, IocArgs iocArgs, object injectTo)
		{
			var builder = new ContainerBuilder();
			var configuration = new IocConfiguration(iocArgs, CommonModule.ToggleManagerForIoc(iocArgs));

			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterType<TenantAuthenticationAlwaysAuthenticated>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterType<CurrentTenantUserFake>().As<ICurrentTenantUser>().SingleInstance();

			builder.RegisterModule(new TestModule());

			registrations?.Invoke(builder);

			Container = builder.Build();

			if (injectTo != null)
				IoCTestService.InjectTo(Container, injectTo);
		}
		
		public static void Setup(Action<ContainerBuilder> registrations, IocArgs iocArgs)
		{
			Setup(registrations, iocArgs, null);
		}

		public static void Setup(Action<ContainerBuilder> registrations)
		{
			Setup(registrations, new IocArgs(new ConfigReader())
			{
				FeatureToggle = TestSiteConfigurationSetup.URL.ToString()
			});
		}

		//should be deleted?
		public static void SetupWithSyncAllEventPublisher(Action<ContainerBuilder> registrations, object injectTo)
		{
			Setup(registrations, new IocArgs(new ConfigReader())
			{
				AllEventPublishingsAsSync = true,
				FeatureToggle = TestSiteConfigurationSetup.URL.ToString()
			}, injectTo);
		}

		public static void Dispose()
		{
			Container?.Dispose();
			Container = null;
		}
	}
}