using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Configuration;
using Autofac.Integration.Mvc;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.IoC;
using Teleopti.Ccc.Web.Areas.Start.Core.IoC;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class ContainerConfiguration : IContainerConfiguration
	{
		private static string AssemblyDirectory
		{
			get
			{
				var uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
				return Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
			}
		}

		#region IContainerConfiguration Members

		public IContainer Configure()
		{
			var builder = new ContainerBuilder();

			builder.RegisterControllers(Assembly.GetExecutingAssembly());

			builder.RegisterModule(new AutofacWebTypesModuleFromRepository20111123());

			builder.RegisterFilterProvider();

			builder.RegisterModule<BootstrapperModule>();

			builder.RegisterModule<CommonModule>();
			builder.RegisterModule<MyTimeAreaModule>();
			builder.RegisterModule<AuthenticationAreaModule>();
			builder.RegisterModule<RuleSetModule>();
			builder.RegisterModule<MobileReportsAreaModule>();


			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<UnitOfWorkModule>();
			builder.RegisterModule<InitializeModule>();
			builder.RegisterModule<AuthenticationModule>();
			builder.RegisterModule<DateAndTimeModule>();
			builder.RegisterModule<LogModule>();

			registerTestDataOverrides(builder);

			return builder.Build();
		}

		#endregion

		// 
		private static void registerTestDataOverrides(ContainerBuilder builder)
		{
			const string configurationFileName = "testdata.autofac.config";
			var configurationFilePath = Path.Combine(AssemblyDirectory, configurationFileName);

			if (File.Exists(configurationFilePath))
			{
				builder.RegisterModule(new ConfigurationSettingsReader("testdata", configurationFilePath));
			}
		}
	}
}
