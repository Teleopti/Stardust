﻿using System.Configuration;
using System.Data.SqlClient;

namespace ManagerTest.Database
{
	public class DatabaseHelper
	{
		private readonly string _connectionString;
		private readonly string _createScriptPath;
		private readonly DatabaseCreator _databaseCreator;
		private readonly string _databaseName;
		private readonly DatabaseSchemaCreator _databaseSchemaCreator;
		private readonly string _masterDatabaseName;
		private readonly string _schemaScriptsPath;

		public DatabaseHelper()
		{
			_connectionString = ConfigurationManager.ConnectionStrings["ManagerConnectionString"].ConnectionString;
			_databaseName = ConfigurationManager.AppSettings["TestDb"];
			_createScriptPath = ConfigurationManager.AppSettings["CreateScriptPath"];
			_schemaScriptsPath = ConfigurationManager.AppSettings["ReleaseScriptPath"];
			_masterDatabaseName = "master";
			var executeMaster = new ExecuteSql(() => openConnection(true));

			_databaseSchemaCreator = new DatabaseSchemaCreator(new SchemaVersionInformation(), executeMaster);
			_databaseCreator = new DatabaseCreator(executeMaster);
		}

		public void Create()
		{
			_databaseCreator.CreateDatabase(_createScriptPath, _databaseName);
			_databaseSchemaCreator.ApplyReleases(_schemaScriptsPath, _databaseName);
		}

		private SqlConnection openConnection(bool masterDb = false)
		{
			var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
			if (masterDb)
			{
				connectionStringBuilder.InitialCatalog = _masterDatabaseName;
			}
			var conn = new SqlConnection(connectionStringBuilder.ConnectionString);
			conn.Open();
			return conn;
		}
	}
}