using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using Manager.Integration.Test.Properties;

namespace Manager.Integration.Test.Helpers
{
    public static class DatabaseHelper
    {

#if (DEBUG)
        private static string _buildMode = "Debug";
#else
        private static string _buildMode = "Release";
#endif

        public static void TryClearDatabase(string connectionStringName = "ManagerConnectionString")
        {
            DirectoryInfo directoryManagerIntegrationConsoleHost =
                new DirectoryInfo(Settings.Default.ManagerIntegrationConsoleHostLocation + _buildMode);

            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = Path.Combine(directoryManagerIntegrationConsoleHost.FullName,
                                                 Settings.Default.ManagerConfigurationFileName )
            };

            if (!File.Exists(configFileMap.ExeConfigFilename))
            {
                return;
            }

            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap,
                                                                                   ConfigurationUserLevel.None);

            var connectionString =
                config.ConnectionStrings.ConnectionStrings[connectionStringName];

            using (SqlConnection connection = new SqlConnection(connectionString.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("truncate table Stardust.JobDefinitions",
                                                           connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new SqlCommand("truncate table Stardust.JobHistory",
                                                           connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new SqlCommand("truncate table Stardust.JobHistoryDetail",
                                                           connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new SqlCommand("truncate table Stardust.WorkerNodes",
                                                           connection))
                {
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
    }
}