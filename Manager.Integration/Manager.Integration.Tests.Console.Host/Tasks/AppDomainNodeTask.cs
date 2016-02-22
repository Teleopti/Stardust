﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Manager.IntegrationTest.Console.Host.Helpers;

namespace Manager.IntegrationTest.Console.Host.Tasks
{
    public class AppDomainNodeTask : IDisposable
    {
        private string BuildMode { get; set; }

        private DirectoryInfo DirectoryNodeAssemblyLocationFullPath { get; set; }

        private FileInfo NodeconfigurationFile { get; set; }

        private string NodeAssemblyName { get; set; }

        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (AppDomainNodeTask));

        public AppDomainNodeTask(string buildMode,
                                 DirectoryInfo directoryNodeAssemblyLocationFullPath,
                                 FileInfo nodeconfigurationFile,
                                 string nodeAssemblyName)
        {
            BuildMode = buildMode;
            DirectoryNodeAssemblyLocationFullPath = directoryNodeAssemblyLocationFullPath;
            NodeconfigurationFile = nodeconfigurationFile;
            NodeAssemblyName = nodeAssemblyName;
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


                Task.Factory.StartNew(() =>
                {
                    var nodeAppDomainSetup = new AppDomainSetup
                    {
                        ApplicationBase = DirectoryNodeAssemblyLocationFullPath.FullName,
                        ApplicationName = NodeAssemblyName,
                        ShadowCopyFiles = "true",
                        ConfigurationFile = NodeconfigurationFile.FullName
                    };

                    MyAppDomain = AppDomain.CreateDomain(NodeconfigurationFile.Name,
                                                         null,
                                                         nodeAppDomainSetup);

                    var assemblyToExecute = new FileInfo(Path.Combine(nodeAppDomainSetup.ApplicationBase,
                                                                      nodeAppDomainSetup.ApplicationName));

                    LogHelper.LogDebugWithLineNumber(Logger,
                                                    "Node (appdomain) will start with friendly name : " + MyAppDomain.FriendlyName);


                    MyAppDomain.ExecuteAssembly(assemblyToExecute.FullName);
                },
                cancellationTokenSource.Token);
            },
            cancellationTokenSource.Token);

            return Task;
        }

        public string GetAppDomainFriendlyName()
        {
            if (MyAppDomain != null)
            {
                return MyAppDomain.FriendlyName;
            }

            return null;
        }

        public void Dispose()
        {
            LogHelper.LogDebugWithLineNumber(Logger,
                                            "Start disposing.");

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

            LogHelper.LogDebugWithLineNumber(Logger,
                                            "Finshed disposing.");
        }
    }
}