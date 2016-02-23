using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Interfaces;
using Manager.IntegrationTest.Console.Host.Properties;

namespace Manager.IntegrationTest.Console.Host.Tasks
{
    public class AppDomainManagerTask : IAppDomain,IDisposable
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (AppDomainManagerTask));

        public AppDomainManagerTask(string buildmode,
                                    DirectoryInfo directoryManagerAssemblyLocationFullPath,
                                    FileInfo configurationFileInfo,
                                    string managerAssemblyName)
        {
            Buildmode = buildmode;
            DirectoryManagerAssemblyLocationFullPath = directoryManagerAssemblyLocationFullPath;
            ConfigurationFileInfo = configurationFileInfo;
            ManagerAssemblyName = managerAssemblyName;
        }

		public string Buildmode { get; set; }
		private DirectoryInfo DirectoryManagerAssemblyLocationFullPath { get; set; }

		private FileInfo ConfigurationFileInfo { get; set; }
		public string ManagerAssemblyName { get; set; }

		public AppDomain MyAppDomain { get; private set; }

		public Task Task { get; private set; }

		private CancellationTokenSource CancellationTokenSource { get; set; }

		public void Dispose()
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start disposing.");

			if (CancellationTokenSource != null &&
			    !CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();
			}

			if (MyAppDomain != null)
			{
				try
				{
					AppDomain.Unload(MyAppDomain);
				}
				catch (Exception)
				{
				}
			}

			if (Task != null)
			{
				Task.Dispose();
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished disposing.");
		}

        public string GetAppDomainFriendlyName()
        {
            if (MyAppDomain != null)
            {
                return MyAppDomain.FriendlyName;
            }

            return null;
        }

        public Task StartTask(CancellationTokenSource cancellationTokenSource)
        {
			Task = Task.Factory.StartNew(() =>
            {
                Task.Factory.StartNew(() =>
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));
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
                        ApplicationName = ManagerAssemblyName,
                        ShadowCopyFiles = "true",
                        ConfigurationFile = ConfigurationFileInfo.FullName
                    };

                    MyAppDomain = AppDomain.CreateDomain(managerAppDomainSetup.ApplicationName,
                                                         null,
                                                         managerAppDomainSetup);

                    var assemblyFile = new FileInfo(Path.Combine(managerAppDomainSetup.ApplicationBase,
                                                                 managerAppDomainSetup.ApplicationName));

                    LogHelper.LogDebugWithLineNumber(Logger,
                                                    "Manager (appdomain) will start with friendly name : " + MyAppDomain.FriendlyName);

                    MyAppDomain.ExecuteAssembly(assemblyFile.FullName);
                },
                cancellationTokenSource.Token);
            }, cancellationTokenSource.Token);

            return Task;
        }
    }
}