using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Configuration;
using Autofac.Integration.Mvc;
using MbCache.Configuration;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference;
using Teleopti.Ccc.Web.Areas.Start.Core.IoC;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class ContainerConfiguration : IContainerConfiguration
	{

		public IContainer Configure()
		{
			var builder = new ContainerBuilder();

			builder.RegisterControllers(Assembly.GetExecutingAssembly());

			builder.RegisterModule(new AutofacWebTypesModuleFromRepository20111123());

			builder.RegisterFilterProvider();

			builder.RegisterModule<BootstrapperModule>();

			builder.RegisterModule<CommonModule>();
			builder.RegisterModule<MyTimeAreaModule>();
			builder.RegisterModule<StartAreaModule>();
			builder.RegisterModule<MobileReportsAreaModule>();

			builder.RegisterModule(new MbCacheModule { Cache = new AspNetCache(20) });
			builder.RegisterModule(new RuleSetModule { CacheRuleSetProjection = false, RegisterRuleSetProjectionService = false });

			builder.RegisterType<RuleSetProjectionService>()
				.Named<IRuleSetProjectionService>("implementor");
			builder.RegisterDecorator<IRuleSetProjectionService>((c, inner) => new RuleSetProjectionServiceForMultiSessionCaching(inner, c.Resolve<ILazyLoadingManager>()), "implementor")
				.As<IRuleSetProjectionService>()
				;

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<UnitOfWorkModule>();
			builder.RegisterModule<InitializeModule>();
			builder.RegisterModule<AuthenticationModule>();
			builder.RegisterModule<DateAndTimeModule>();
			builder.RegisterModule<LogModule>();

			registerTestDataOverrides(builder);

			return builder.Build();
		}

		private static void registerTestDataOverrides(ContainerBuilder builder)
		{
			const string configurationFileName = "testdata.autofac.config";
			var configurationFilePath = Path.Combine(AssemblyDirectory, configurationFileName);

			if (File.Exists(configurationFilePath))
			{
				builder.RegisterModule(new ConfigurationSettingsReader("testdata", configurationFilePath));
			}
		}

		private static string AssemblyDirectory
		{
			get
			{
				var uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
				return Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
			}
		}
	}
}
