using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.Integration.Test.Properties;

namespace Manager.Integration.Test.Tasks
{
	public class AppDomainTask : IDisposable
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (AppDomainTask));

		public AppDomainTask(string buildmode)
		{
			Buildmode = buildmode;
		}

		public string Buildmode { get; set; }

		private AppDomain MyAppDomain { get; set; }

		public Task Task { get; private set; }

		private CancellationTokenSource CancellationTokenSource { get; set; }

		public void Dispose()
		{
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
		}

		public Task StartTask(CancellationTokenSource cancellationTokenSource,
		                      int numberOfNodes)
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
					var assemblyLocationFullPath =
						Path.Combine(Settings.Default.ManagerIntegrationConsoleHostLocation,
						             Buildmode);

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

					MyAppDomain = AppDomain.CreateDomain(managerAppDomainSetup.ApplicationName,
					                                     AppDomain.CurrentDomain.Evidence,
					                                     managerAppDomainSetup);

					var assemblyToExecute =
						new FileInfo(Path.Combine(managerAppDomainSetup.ApplicationBase,
						                          managerAppDomainSetup.ApplicationName));

					MyAppDomain.ExecuteAssembly(assemblyToExecute.FullName,
					                            managerAppDomainSetup.AppDomainInitializerArguments);
				}, cancellationTokenSource.Token);
			}, cancellationTokenSource.Token);

			return Task;
		}
	}
}