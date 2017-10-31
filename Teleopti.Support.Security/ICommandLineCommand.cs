using System;
using System.Data.SqlClient;
using System.Globalization;
using log4net;

namespace Teleopti.Support.Security
{
    internal interface ICommandLineCommand
    {
        int Execute(IDatabaseArguments databaseArguments);
    }

	internal abstract class CommandLineCommandWithFixChecker : ICommandLineCommand
	{
		protected readonly int specialVersion;
		private static readonly ILog log = LogManager.GetLogger(typeof(CommandLineCommandWithFixChecker));

		protected CommandLineCommandWithFixChecker(int specialVersion)
		{
			this.specialVersion = specialVersion;
		}
		
		protected bool isFixApplied(SqlConnection connection)
		{
			using (var command = connection.CreateCommand())
			{
				command.CommandText = $"SELECT COUNT(*) FROM dbo.DatabaseVersion WHERE BuildNumber=('{-specialVersion}')";
				var versionCount = (int)command.ExecuteScalar();
				if (versionCount > 0)
					return true;
			}
			return false;
		}
		
		protected void setApplied(SqlConnection connection, SqlTransaction transaction)
		{
			log.Debug("Updating database version...");
			executeNonQuery(connection, transaction, string.Format(CultureInfo.InvariantCulture,
				$"INSERT INTO dbo.DatabaseVersion (BuildNumber,SystemVersion,AddedDate,AddedBy) VALUES ('{-specialVersion}','8.4.{specialVersion}.1',GetDate(),'{0}')",
				Environment.UserName));
			log.Debug("Done updating database version.");
		}
		
		protected static void executeNonQuery(SqlConnection connection, SqlTransaction transaction, string query)
		{
			var command = connection.CreateCommand();
			command.CommandText = query;
			command.Transaction = transaction;
			command.ExecuteNonQuery();
		}

		public abstract int Execute(IDatabaseArguments databaseArguments);
	}
}