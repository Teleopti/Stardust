using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Manager.IntegrationTest.ConsoleHost.Interfaces;
using Manager.IntegrationTest.ConsoleHost.Log4Net;

namespace Manager.IntegrationTest.ConsoleHost.Tasks
{
	public class AppDomainManagerTask : IAppDomain,
		IDisposable
	{
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

		public string GetAppDomainUniqueId()
		{
			if (ConfigurationFileInfo != null)
			{
				return ConfigurationFileInfo.Name;
			}

			return null;
		}

		public void Dispose()
		{
			this.Log().DebugWithLineNumber("Start disposing.");

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

			this.Log().DebugWithLineNumber("Finished disposing.");
		}

		public Task StartTask(CancellationTokenSource cancellationTokenSource)
		{
			Task = Task.Run(() =>
			{
				Task.Run(() =>
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

				Task.Run(() =>
				{
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

					this.Log().DebugWithLineNumber("Manager (appdomain) will start with friendly name : " + MyAppDomain.FriendlyName);

					MyAppDomain.ExecuteAssembly(assemblyFile.FullName);
				}, cancellationTokenSource.Token);
			}, cancellationTokenSource.Token);

			return Task;
		}
	}
}