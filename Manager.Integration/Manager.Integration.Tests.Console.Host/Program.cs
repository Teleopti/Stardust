using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Manager.IntegrationTest.Console.Host.Properties;

namespace Manager.IntegrationTest.Console.Host
{
    public static class Program
    {
        private const string CopiedManagerConfigName = "Manager.config";

#if (DEBUG)
        private static string _buildMode = "Debug";
#else
        private static string _buildMode = "Release";
#endif

        public static FileInfo ManagerConfigurationFile { get; set; }

        public static FileInfo CopiedManagerConfigurationFile { get; set; }


        private static string AddEndingSlash(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!path.EndsWith(@"\"))
                {
                    path += @"\";
                }
            }

            return path;
        }

        private static void Main(string[] args)
        {
            var directoryManagerConfigurationFileFullPath =
                new DirectoryInfo(AddEndingSlash(Settings.Default.ManagerConfigurationFileFullPath + _buildMode));

            ManagerConfigurationFile =
                new FileInfo(directoryManagerConfigurationFileFullPath.FullName +
                             Settings.Default.ManagerConfigurationFileName);

            CopiedManagerConfigurationFile =
                CopyManagerConfigurationFile(ManagerConfigurationFile,
                    CopiedManagerConfigName);

            var directoryNodeConfigurationFileFullPath =
                new DirectoryInfo(AddEndingSlash(Settings.Default.NodeConfigurationFileFullPath + _buildMode));

            var nodeConfigurationFile =
                new FileInfo(directoryNodeConfigurationFileFullPath.FullName +
                             Settings.Default.NodeConfigurationFileName);

            var nodeconfigurationFiles = new Dictionary<string, FileInfo>();

            var portStartNumber =
                Settings.Default.NodeEndpointPortNumberStart;

            var numberOfNodesToStart = Settings.Default.NumberOfNodesToStart;

            if (args.Any())
            {
                numberOfNodesToStart = Convert.ToInt32(args[0]);
            }

            if (numberOfNodesToStart > 0)
            {
                for (var i = 1; i <= numberOfNodesToStart; i++)
                {
                    var nodeName = "Node" + i;

                    var configName = nodeName + ".config";

                    var portNumber = portStartNumber + (i - 1);

                    var endPointUri =
                        new Uri(Settings.Default.NodeEndpointUriTemplate.Replace("PORTNUMBER",
                            portNumber.ToString()));

                    var copiedConfigurationFile =
                        CreateNodeConfigFile(nodeConfigurationFile,
                            configName,
                            nodeName,
                            new Uri(Settings.Default.ManagerLocationUri),
                            endPointUri,
                            Settings.Default.HandlerAssembly);

                    nodeconfigurationFiles.Add(nodeName,
                        copiedConfigurationFile);
                }
            }


            var tasks = new List<Task>();

            var managerTask = new Task(() =>
            {
                var managerAssemblyLocationFullPath = Settings.Default.ManagerAssemblyLocationFullPath;

                var directoryManagerAssemblyLocationFullPath =
                    new DirectoryInfo(AddEndingSlash(managerAssemblyLocationFullPath + _buildMode));

                // Start manager.
                var managerAppDomainSetup = new AppDomainSetup
                {
                    ApplicationBase = directoryManagerAssemblyLocationFullPath.FullName,
                    ApplicationName = Settings.Default.ManagerAssemblyName,
                    ShadowCopyFiles = "true",
                    ConfigurationFile = CopiedManagerConfigurationFile.FullName
                };

                var managerAppDomain = AppDomain.CreateDomain(managerAppDomainSetup.ApplicationName,
                    null,
                    managerAppDomainSetup);

                managerAppDomain.ExecuteAssembly(managerAppDomainSetup.ApplicationBase +
                                                 managerAppDomainSetup.ApplicationName);
            });

            tasks.Add(managerTask);

            var directoryNodeAssemblyLocationFullPath =
                new DirectoryInfo(AddEndingSlash(Settings.Default.NodeAssemblyLocationFullPath + _buildMode));

            foreach (var nodeconfigurationFile in nodeconfigurationFiles)
            {
                var nodeTask = new Task(() =>
                {
                    var nodeAppDomainSetup = new AppDomainSetup
                    {
                        ApplicationBase = directoryNodeAssemblyLocationFullPath.FullName,
                        ApplicationName = Settings.Default.NodeAssemblyName,
                        ShadowCopyFiles = "true",
                        ConfigurationFile = nodeconfigurationFile.Value.FullName
                    };

                    var nodeAppDomain = AppDomain.CreateDomain(nodeconfigurationFile.Key,
                        null,
                        nodeAppDomainSetup);

                    var assemblyToExecute =
                        nodeAppDomainSetup.ApplicationBase + nodeAppDomainSetup.ApplicationName;

                    nodeAppDomain.ExecuteAssembly(assemblyToExecute);
                });

                tasks.Add(nodeTask);
            }

            foreach (var task in tasks)
            {
                task.Start();
            }

            System.Console.ReadKey();
        }

        public static FileInfo CreateNodeConfigFile(FileInfo nodeConfigurationFile,
            string newConfigurationFileName,
            string nodeName,
            Uri managerEndpoint,
            Uri nodeEndPoint,
            string handlerAssembly)
        {
            var copiedNodeConfigFile = nodeConfigurationFile.CopyTo(newConfigurationFileName,
                true);

            // Change app settings.
            var configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = copiedNodeConfigFile.FullName
            };

            var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap,
                ConfigurationUserLevel.None);

            config.AppSettings.Settings["NodeName"].Value = nodeName;
            config.AppSettings.Settings["BaseAddress"].Value = nodeEndPoint.ToString();
            config.AppSettings.Settings["ManagerLocation"].Value = managerEndpoint.ToString();
            config.AppSettings.Settings["HandlerAssembly"].Value = handlerAssembly;

            config.Save();

            return copiedNodeConfigFile;
        }

        public static FileInfo CopyManagerConfigurationFile(FileInfo managerConfigFile,
            string newConfigFileName)
        {
            return managerConfigFile.CopyTo(newConfigFileName,
                true);
        }
    }
}