using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.Integration.Test.Properties;

namespace Manager.Integration.Test.Helpers
{
	public static class AppDomainHelper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (AppDomainHelper));

		static AppDomainHelper()
		{
			AppDomains = new ConcurrentDictionary<string, AppDomain>();
		}

		public static ConcurrentDictionary<string, AppDomain> AppDomains { get; set; }

		public static Task CreateAppDomainForManagerIntegrationConsoleHost(string buildmode,
		                                                                   int numberOfNodes,
		                                                                   CancellationTokenSource cancellationTokenSource)
		{
			return new Task(() =>
			{
				var assemblyLocationFullPath =
					Path.Combine(Settings.Default.ManagerIntegrationConsoleHostLocation,
					             buildmode);

				var directoryManagerAssemblyLocationFullPath =
					new DirectoryInfo(assemblyLocationFullPath);

				var configFileName =
					new FileInfo(Path.Combine(directoryManagerAssemblyLocationFullPath.FullName,
					                          Settings.Default.ManagerIntegrationConsoleHostConfigurationFile));

				var managerAppDomainSetup = new AppDomainSetup
				{
					ApplicationBase = directoryManagerAssemblyLocationFullPath.FullName,
					ApplicationName = Settings.Default.ManagerIntegrationConsoleHostAssemblyName,
					ShadowCopyFiles = "true",
					AppDomainInitializerArguments = new[]
					{
						numberOfNodes.ToString()
					},
					ConfigurationFile = configFileName.FullName
				};

				var appDomain = AppDomain.CreateDomain(managerAppDomainSetup.ApplicationName,
				                                       AppDomain.CurrentDomain.Evidence,
				                                       managerAppDomainSetup);

				var assemblyToExecute =
					new FileInfo(Path.Combine(managerAppDomainSetup.ApplicationBase,
					                          managerAppDomainSetup.ApplicationName));

				LogHelper.LogDebugWithLineNumber(
					"Try start ManagerIntegrationConsoleHost (appdomain) with friendly name " + appDomain.FriendlyName,
					Logger);

				appDomain.ExecuteAssembly(assemblyToExecute.FullName,
				                          managerAppDomainSetup.AppDomainInitializerArguments);
			},
			                cancellationTokenSource.Token);
		}

		public static void AddOrUpdateAppDomains(string key,
		                                         AppDomain value)
		{
			AppDomains.AddOrUpdate(key,
			                       value,
			                       (name,
			                        val) => value);
		}
	}
}