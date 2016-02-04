using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Manager.Integration.Test.Properties;

namespace Manager.Integration.Test.Helpers
{
    public static class AppDomainHelper
    {
        public static ConcurrentDictionary<string, AppDomain> AppDomains { get; private set; }

        static AppDomainHelper()
        {
            AppDomains = new ConcurrentDictionary<string, AppDomain>();
        }

        public static Task CreateAppDomainForManagerIntegrationConsoleHost(string buildmode,
                                                                           int numberOfNodes)
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
                    AppDomainInitializerArguments = new[] {numberOfNodes.ToString()},
                    ConfigurationFile = configFileName.FullName,
                };

                AppDomain appDomain = AppDomain.CreateDomain(managerAppDomainSetup.ApplicationName,
                                                             AppDomain.CurrentDomain.Evidence,
                                                             managerAppDomainSetup);

                AddOrUpdateAppDomains(managerAppDomainSetup.ApplicationName,
                                      appDomain);

                FileInfo assemblyToExecute =
                    new FileInfo(Path.Combine(managerAppDomainSetup.ApplicationBase,
                                              managerAppDomainSetup.ApplicationName));

                var ret = appDomain.ExecuteAssembly(assemblyToExecute.FullName,
                                                    managerAppDomainSetup.AppDomainInitializerArguments);

                Environment.ExitCode = ret;
            });
        }

        public static void AddOrUpdateAppDomains(string key,
                                                 AppDomain value)
        {
            AppDomain appdomain = AppDomains.AddOrUpdate(key,
                                                         value,
                                                         (name,
                                                          val) => value);
        }
    }
}