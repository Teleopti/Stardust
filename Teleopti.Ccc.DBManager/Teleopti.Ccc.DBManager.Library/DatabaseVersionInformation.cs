using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseVersionInformation
	{
		private readonly DatabaseFolder _databaseFolder;
		private readonly SqlConnection _connection;

		public DatabaseVersionInformation(DatabaseFolder databaseFolder, SqlConnection connection)
		{
			_databaseFolder = databaseFolder;
			_connection = connection;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public void CreateTable()
		{
			var path = _databaseFolder.CreateScriptsPath();
			var scriptFile = Path.Combine(path, "CreateDatabaseVersion.sql");
			var script = File.ReadAllText(scriptFile);
			using (var sqlCommand = new SqlCommand(script, _connection))
			{
				sqlCommand.ExecuteNonQuery();
			}
		}

		public int GetDatabaseVersion()
		{
			using (var sqlCommand = new SqlCommand("SELECT MAX(BuildNumber) FROM dbo.[DatabaseVersion]", _connection))
			{
				return (int)sqlCommand.ExecuteScalar();
			}
		}

	}
}
