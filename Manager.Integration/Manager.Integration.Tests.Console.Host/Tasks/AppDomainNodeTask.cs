using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.IntegrationTest.Console.Host.Helpers;
using Manager.IntegrationTest.Console.Host.Properties;

namespace Manager.IntegrationTest.Console.Host.Tasks
{
    public class AppDomainNodeTask : IDisposable
    {
        private string BuildMode { get; set; }

        private DirectoryInfo DirectoryNodeAssemblyLocationFullPath { get; set; }

        private FileInfo NodeconfigurationFile { get; set; }

        public string NodeAssemblyName { get; set; }

        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (AppDomainNodeTask));

        public AppDomainNodeTask(string buildMode,
                                 DirectoryInfo directoryNodeAssemblyLocationFullPath,
                                 FileInfo nodeconfigurationFile)
        {
            BuildMode = buildMode;
            DirectoryNodeAssemblyLocationFullPath = directoryNodeAssemblyLocationFullPath;
            NodeconfigurationFile = nodeconfigurationFile;
        }

        private AppDomain MyAppDomain { get; set; }

        public Task Task { get; private set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        public Task StartTask(CancellationTokenSource cancellationTokenSource)
        {
            Task = Task.Factory.StartNew(() =>
            {
                Task.Factory.StartNew(() =>
                {
                    while (!CancellationTokenSource.IsCancellationRequested)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    }

                    if (CancellationTokenSource.IsCancellationRequested)
                    {
                        CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                },
                cancellationTokenSource.Token);


                Task.Factory.StartNew(() =>
                {
                    var nodeAppDomainSetup = new AppDomainSetup
                    {
                        ApplicationBase = DirectoryNodeAssemblyLocationFullPath.FullName,
                        ApplicationName = Settings.Default.NodeAssemblyName,
                        ShadowCopyFiles = "true",
                        ConfigurationFile = NodeconfigurationFile.FullName
                    };

                    MyAppDomain = AppDomain.CreateDomain(NodeconfigurationFile.Name,
                                                         null,
                                                         nodeAppDomainSetup);

                    var assemblyToExecute = new FileInfo(Path.Combine(nodeAppDomainSetup.ApplicationBase,
                                                                      nodeAppDomainSetup.ApplicationName));

                    LogHelper.LogInfoWithLineNumber(Logger,
                                                    "Node (appdomain) will start with friendly name : " + MyAppDomain.FriendlyName);


                    MyAppDomain.ExecuteAssembly(assemblyToExecute.FullName);
                },
                cancellationTokenSource.Token);
            },
            cancellationTokenSource.Token);

            return Task;
        }


        public void Dispose()
        {
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
        }
    }
}