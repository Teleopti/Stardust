using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Properties;

namespace Manager.IntegrationTest.Console.Host.Tasks
{
    public class AppDomainManagerTask : IDisposable
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (AppDomainManagerTask));

        public string Buildmode { get; set; }
        private DirectoryInfo DirectoryManagerAssemblyLocationFullPath { get; set; }

        private FileInfo ConfigurationFileInfo { get; set; }

        public AppDomainManagerTask(string buildmode,
                                    DirectoryInfo directoryManagerAssemblyLocationFullPath,
                                    FileInfo configurationFileInfo)
        {
            Buildmode = buildmode;
            DirectoryManagerAssemblyLocationFullPath = directoryManagerAssemblyLocationFullPath;
            ConfigurationFileInfo = configurationFileInfo;
        }

        public string GetAppDomainFriendlyName()
        {
            if (MyAppDomain != null)
            {
                return MyAppDomain.FriendlyName;
            }

            return null;
        }

        public AppDomain MyAppDomain { get; private set; }

        public Task Task { get; private set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        public Task StartTask(CancellationTokenSource cancellationTokenSource)
        {
            Task= Task.Factory.StartNew(() =>
            {
                Task.Factory.StartNew(() =>
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    }

                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }

                }, cancellationTokenSource.Token);

                Task.Factory.StartNew(() =>
                {
                    DirectoryManagerAssemblyLocationFullPath =
                        new DirectoryInfo(Path.Combine(Settings.Default.ManagerAssemblyLocationFullPath,
                                                       Buildmode));

                    // Start manager.
                    var managerAppDomainSetup = new AppDomainSetup
                    {
                        ApplicationBase = DirectoryManagerAssemblyLocationFullPath.FullName,
                        ApplicationName = Settings.Default.ManagerAssemblyName,
                        ShadowCopyFiles = "true",
                        ConfigurationFile = ConfigurationFileInfo.FullName
                    };

                    MyAppDomain = AppDomain.CreateDomain(managerAppDomainSetup.ApplicationName,
                                                         null,
                                                         managerAppDomainSetup);

                    var assemblyFile = new FileInfo(Path.Combine(managerAppDomainSetup.ApplicationBase,
                                                                 managerAppDomainSetup.ApplicationName));

                    LogHelper.LogInfoWithLineNumber(Logger,
                                                    "Manager (appdomain) will start with friendly name : " + MyAppDomain.FriendlyName);

                    MyAppDomain.ExecuteAssembly(assemblyFile.FullName);

                },
                cancellationTokenSource.Token);

            }, cancellationTokenSource.Token);

            return Task;
        }

        public void Dispose()
        {
            LogHelper.LogInfoWithLineNumber(Logger, "Start disposing.");

            if (CancellationTokenSource != null &&
                !CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel();
            }

            if (MyAppDomain != null)
            {
                AppDomain.Unload(MyAppDomain);
            }

            if (Task != null)
            {
                Task.Dispose();
            }

            LogHelper.LogInfoWithLineNumber(Logger, "Finished disposing.");
        }
    }
}