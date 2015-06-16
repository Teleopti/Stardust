using System;
using System.Data.SqlClient;
using log4net;

namespace Teleopti.Support.Security
{
	internal class PasswordEncryption : ICommandLineCommand
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));

		public int Execute(CommandLineArgument commandLineArgument)
		{
			//Select database version 
			using (SqlConnection connection = new SqlConnection(commandLineArgument.ApplicationDbConnectionString()))
			{
				try
				{
					connection.Open();
				}
				catch (SqlException ex)
				{
					log.Debug("Could not open Sql Connection. Error message: " + ex.Message);
					return 1;
				}
				catch (InvalidOperationException ex)
				{
					log.Debug("Could not open Sql Connection. Error message: " + ex.Message);
					return 1;
				}
				//Check version
				var command = connection.CreateCommand();
				command.CommandText = "SELECT COUNT(*) FROM dbo.DatabaseVersion WHERE BuildNumber=('-290')";
				var versionCount = (int)command.ExecuteScalar();
				if (versionCount > 0)
				{
					return 0;
				}

				log.Error("Cannot upgrade directly to this version. Please upgrade to latest version before 8.2.427.35416 first!");
				return 1;
			}
		}
	}
}