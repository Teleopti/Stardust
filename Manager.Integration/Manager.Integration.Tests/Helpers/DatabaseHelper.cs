using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using Manager.Integration.Test.Properties;

namespace Manager.Integration.Test.Helpers
{
    public static class DatabaseHelper
    {
        public static void ClearDatabase(string connectionStringName = "ManagerConnectionString")
        {
            DirectoryInfo directoryManagerIntegrationConsoleHost =
                new DirectoryInfo(Settings.Default.ManagerIntegrationConsoleHostLocation);


            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = Path.Combine(directoryManagerIntegrationConsoleHost.FullName,
                                                 Settings.Default.ManagerConfigurationFileName)
            };

            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap,
                                                                                   ConfigurationUserLevel.None);

            var connectionString =
                config.ConnectionStrings.ConnectionStrings[connectionStringName];

            using (SqlConnection connection = new SqlConnection(connectionString.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("truncate table JobDefinitions",
                                                           connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new SqlCommand("truncate table JobHistory",
                                                           connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new SqlCommand("truncate table JobHistoryDetail",
                                                           connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SqlCommand command = new SqlCommand("truncate table WorkerNodes",
                                                           connection))
                {
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
    }
}