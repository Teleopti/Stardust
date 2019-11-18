using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Manager.IntegrationTest.Console.Host.Interfaces;
using Manager.IntegrationTest.Console.Host.Log4Net;

namespace Manager.IntegrationTest.Console.Host.Tasks
{
	public class AppDomainNodeTask : IAppDomain, 
									 IDisposable
    {
        private readonly DirectoryInfo _directoryNodeAssemblyLocationFullPath;
        private readonly FileInfo _nodeconfigurationFile;
        private readonly string _nodeAssemblyName;
        private readonly ConcurrentStack<AppDomain> _myAppDomain = new ConcurrentStack<AppDomain>();

        private Task _task;

        public AppDomainNodeTask(DirectoryInfo directoryNodeAssemblyLocationFullPath,
		                         FileInfo nodeconfigurationFile,
		                         string nodeAssemblyName)
		{
			_directoryNodeAssemblyLocationFullPath = directoryNodeAssemblyLocationFullPath;
			_nodeconfigurationFile = nodeconfigurationFile;
			_nodeAssemblyName = nodeAssemblyName;
		}

		public string GetAppDomainUniqueId()
		{
            return _nodeconfigurationFile?.Name;
        }

		public void Dispose()
		{
			this.Log().DebugWithLineNumber("Start disposing.");

			while (_myAppDomain.TryPop(out var domain))
			{
				try
				{
					AppDomain.Unload(domain);
				}

				catch (Exception)
				{
				}
			}

            _task?.Dispose();

            this.Log().DebugWithLineNumber("Finished disposing.");
		}

		public Task StartTask(CancellationTokenSource cancellationTokenSource)
        {
            _task = Task.Run(() =>
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
                        },
                        cancellationTokenSource.Token);


                    Task.Run(() =>
                        {
                            var nodeAppDomainSetup = new AppDomainSetup
                            {
                                ApplicationBase = _directoryNodeAssemblyLocationFullPath.FullName,
                                ApplicationName = _nodeAssemblyName,
                                ShadowCopyFiles = "true",
                                ConfigurationFile = _nodeconfigurationFile.FullName
                            };

                            var myAppDomain = AppDomain.CreateDomain(_nodeconfigurationFile.Name,
                                null,
                                nodeAppDomainSetup);

                            var assemblyToExecute = new FileInfo(Path.Combine(nodeAppDomainSetup.ApplicationBase,
                                nodeAppDomainSetup.ApplicationName));

                            this.Log().DebugWithLineNumber(
                                $"Node (appdomain) will start with friendly name : {myAppDomain.FriendlyName}");
                            
                            myAppDomain.ExecuteAssembly(assemblyToExecute.FullName);
                            _myAppDomain.Push(myAppDomain);
                        },
                        cancellationTokenSource.Token);
                },
                cancellationTokenSource.Token);

			return _task;
		}
	}
}