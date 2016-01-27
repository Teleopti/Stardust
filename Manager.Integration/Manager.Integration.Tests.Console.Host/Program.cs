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

        private const string CopiedManagerConfigName = "Manager.config";

        public static FileInfo ManagerConfigurationFile { get; set; }

        public static FileInfo CopiedManagerConfigurationFile { get; set; }

        static void Main(string[] args)
        {
            DirectoryInfo directoryManagerConfigurationFileFullPath =
                new DirectoryInfo(AddEndingSlash(Settings.Default.ManagerConfigurationFileFullPath));

            ManagerConfigurationFile =
                new FileInfo(directoryManagerConfigurationFileFullPath.FullName + Settings.Default.ManagerConfigurationFileName);

            CopiedManagerConfigurationFile =
                CopyManagerConfigurationFile(ManagerConfigurationFile,
                                             CopiedManagerConfigName);

            DirectoryInfo directoryNodeConfigurationFileFullPath =
                new DirectoryInfo(AddEndingSlash(Settings.Default.NodeConfigurationFileFullPath));

            FileInfo nodeConfigurationFile =
                new FileInfo(directoryNodeConfigurationFileFullPath.FullName + Settings.Default.NodeConfigurationFileName);

            Dictionary<string, FileInfo> nodeconfigurationFiles = new Dictionary<string, FileInfo>();

            int portStartNumber =
                Settings.Default.NodeEndpointPortNumberStart;

            int numberOfNodesToStart = Settings.Default.NumberOfNodesToStart;

            if (args.Any())
            {
                numberOfNodesToStart = Convert.ToInt32(args[0]);
            }

            if (numberOfNodesToStart > 0)
            {
                for (int i = 1; i <= numberOfNodesToStart; i++)
                {
                    string nodeName = "Node" + i;

                    string configName = nodeName + ".config";

                    int portNumber = portStartNumber + (i - 1);

                    Uri endPointUri =
                        new Uri(Settings.Default.NodeEndpointUriTemplate.Replace("PORTNUMBER",
                                                                                 portNumber.ToString()));

                    FileInfo copiedConfigurationFile =
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


            List<Task> tasks = new List<Task>();

            var managerTask = new Task(() =>
            {
                DirectoryInfo directoryManagerAssemblyLocationFullPath =
                    new DirectoryInfo(AddEndingSlash(Settings.Default.ManagerAssemblyLocationFullPath));

                // Start manager.
                var managerAppDomainSetup = new AppDomainSetup
                {
                    ApplicationBase = directoryManagerAssemblyLocationFullPath.FullName,
                    ApplicationName = Settings.Default.ManagerAssemblyName,
                    ShadowCopyFiles = "true",
                    ConfigurationFile = CopiedManagerConfigurationFile.FullName
                };

                AppDomain managerAppDomain = AppDomain.CreateDomain(managerAppDomainSetup.ApplicationName,
                                                                    null,
                                                                    managerAppDomainSetup);

                managerAppDomain.ExecuteAssembly(managerAppDomainSetup.ApplicationBase + managerAppDomainSetup.ApplicationName);
            });

            tasks.Add(managerTask);

            DirectoryInfo directoryNodeAssemblyLocationFullPath = new DirectoryInfo(AddEndingSlash(Settings.Default.NodeAssemblyLocationFullPath));

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

                    AppDomain nodeAppDomain = AppDomain.CreateDomain(nodeconfigurationFile.Key,
                                                                     null,
                                                                     nodeAppDomainSetup);

                    string assemblyToExecute =
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
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = copiedNodeConfigFile.FullName
            };

            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap,
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