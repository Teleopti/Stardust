using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using log4net;
using Manager.Integration.Test.Properties;

namespace Manager.Integration.Test.Helpers
{
    public static class DatabaseHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DatabaseHelper));

#if (DEBUG)
        private static string _buildMode = "Debug";
#else
        private static string _buildMode = "Release";
#endif

        public static void TryClearDatabase(string connectionStringName = "ManagerConnectionString")
        {
            LogHelper.LogInfoWithLineNumber("Start.",Logger);

            DirectoryInfo directoryManagerIntegrationConsoleHost =
                new DirectoryInfo(Settings.Default.ManagerIntegrationConsoleHostLocation + _buildMode);

            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = Path.Combine(directoryManagerIntegrationConsoleHost.FullName,
                                                 Settings.Default.ManagerConfigurationFileName )
            };

            if (!File.Exists(configFileMap.ExeConfigFilename))
            {
                LogHelper.LogErrorWithLineNumber(configFileMap.ExeConfigFilename +  " does not exists.", Logger);

                return;
            }            

            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap,
                                                                                   ConfigurationUserLevel.None);

            LogHelper.LogInfoWithLineNumber("Open configuration file : " + config.FilePath, Logger);

            LogHelper.LogInfoWithLineNumber("Get connection string for : " + connectionStringName, Logger);

            var connectionString =
                config.ConnectionStrings.ConnectionStrings[connectionStringName];
            
            using (SqlConnection connection = new SqlConnection(connectionString.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("truncate table Stardust.JobDefinitions",
                                                           connection))
                {
                    LogHelper.LogInfoWithLineNumber(command.CommandText, Logger);

                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new SqlCommand("truncate table Stardust.JobHistory",
                                                           connection))
                {
                    LogHelper.LogInfoWithLineNumber(command.CommandText, Logger);

                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new SqlCommand("truncate table Stardust.JobHistoryDetail",
                                                           connection))
                {
                    LogHelper.LogInfoWithLineNumber(command.CommandText, Logger);

                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new SqlCommand("truncate table Stardust.WorkerNodes",
                                                           connection))
                {
                    LogHelper.LogInfoWithLineNumber(command.CommandText, Logger);

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
    }
}