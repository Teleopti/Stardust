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
	}
}
