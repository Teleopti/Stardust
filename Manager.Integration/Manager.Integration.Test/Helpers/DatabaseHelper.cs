using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using log4net;
using Manager.Integration.Test.Properties;

namespace Manager.Integration.Test.Helpers
{
	public static class DatabaseHelper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (DatabaseHelper));

#if (DEBUG)
		private static readonly string _buildMode = "Debug";
#else
        private static string _buildMode = "Release";
#endif

		public static void TryClearDatabase(string connectionStringName = "ManagerConnectionString")
		{
			LogHelper.LogDebugWithLineNumber("Start.", Logger);

			var directoryManagerIntegrationConsoleHost =
				new DirectoryInfo(Settings.Default.ManagerIntegrationConsoleHostLocation + _buildMode);

			var configFileMap = new ExeConfigurationFileMap
			{
				ExeConfigFilename = Path.Combine(directoryManagerIntegrationConsoleHost.FullName,
				                                 Settings.Default.ManagerConfigurationFileName)
			};

			if (!File.Exists(configFileMap.ExeConfigFilename))
			{
				LogHelper.LogErrorWithLineNumber(configFileMap.ExeConfigFilename + " does not exists.", Logger);

				return;
			}

			var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap,
			                                                             ConfigurationUserLevel.None);

			LogHelper.LogDebugWithLineNumber("Open configuration file : " + config.FilePath, Logger);

			LogHelper.LogDebugWithLineNumber("Get connection string for : " + connectionStringName, Logger);

			var connectionString =
				config.ConnectionStrings.ConnectionStrings[connectionStringName];
			
			using (var connection = new SqlConnection(connectionString.ConnectionString))
			{
				connection.Open();
				
					using (var command = new SqlCommand("truncate table Stardust.JobDefinitions",
													connection))
					{
						LogHelper.LogDebugWithLineNumber(command.CommandText, Logger);

						command.ExecuteNonQuery();
					}
				
					using (var command = new SqlCommand("truncate table Stardust.JobHistory",
													connection))
					{
						LogHelper.LogDebugWithLineNumber(command.CommandText, Logger);

						command.ExecuteNonQuery();
					}
			
					using (var command = new SqlCommand("truncate table Stardust.JobHistoryDetail",
													connection))
					{
						LogHelper.LogDebugWithLineNumber(command.CommandText, Logger);

						command.ExecuteNonQuery();
					}
				
					using (var command = new SqlCommand("truncate table Stardust.WorkerNodes",
													connection))
					{
						LogHelper.LogDebugWithLineNumber(command.CommandText, Logger);

						command.ExecuteNonQuery();
					}
				
				connection.Close();
				LogHelper.LogDebugWithLineNumber("Stop.", Logger);
			}
		}
	}
}