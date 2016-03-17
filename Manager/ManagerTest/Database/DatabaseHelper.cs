﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using log4net;
using Stardust.Manager.Extensions;
using Stardust.Manager.Helpers;


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
			var executeMaster = new ExecuteSql(() => OpenConnection(true));

			_databaseSchemaCreator = new DatabaseSchemaCreator(new SchemaVersionInformation(), executeMaster);
			_databaseCreator = new DatabaseCreator(executeMaster);
		}

		public void Create()
		{
			_databaseCreator.CreateDatabase(_createScriptPath, _databaseName);
			_databaseSchemaCreator.ApplyReleases(_schemaScriptsPath, _databaseName);
		}

		private SqlConnection OpenConnection(bool masterDb = false)
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

		public void TryClearDatabase()
		{
			this.Log().DebugWithLineNumber("Start truncating database tables.");

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();

				//-------------------------------------------------
				// Truncate table Stardust.JobDefinitions.
				//-------------------------------------------------
				using (var command = new SqlCommand("truncate table Stardust.JobDefinitions",
				                                    connection))
				{
					this.Log().DebugWithLineNumber("Start: " + command.CommandText);

					command.ExecuteNonQuery();

					this.Log().DebugWithLineNumber("Finished: " + command.CommandText);
				}

				//-------------------------------------------------
				// Truncate table Stardust.JobHistory.
				//-------------------------------------------------
				using (var command = new SqlCommand("truncate table Stardust.JobHistory",
				                                    connection))
				{
					this.Log().DebugWithLineNumber("Start: " + command.CommandText);

					command.ExecuteNonQuery();

					this.Log().DebugWithLineNumber("Finished: " + command.CommandText);
				}

				//-------------------------------------------------
				// Truncate table Stardust.JobHistoryDetail.
				//-------------------------------------------------
				using (var command = new SqlCommand("truncate table Stardust.JobHistoryDetail",
				                                    connection))
				{
					this.Log().DebugWithLineNumber("Start: " + command.CommandText);

					command.ExecuteNonQuery();

					this.Log().DebugWithLineNumber("Finished: " + command.CommandText);
				}

				//-------------------------------------------------
				// Truncate table Stardust.WorkerNodes.
				//-------------------------------------------------
				using (var command = new SqlCommand("truncate table Stardust.WorkerNodes",
				                                    connection))
				{
					this.Log().DebugWithLineNumber("Start: " + command.CommandText);

					command.ExecuteNonQuery();

					this.Log().DebugWithLineNumber("Finished: " + command.CommandText);
				}

				connection.Close();

				this.Log().DebugWithLineNumber("Finished truncating database tables.");
			}
		}
	}
}