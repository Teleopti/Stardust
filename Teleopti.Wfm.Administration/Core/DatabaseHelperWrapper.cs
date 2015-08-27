﻿using System;
using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public interface IDatabaseHelperWrapper
	{
		DbCheckResultModel Exists(string databaseConnectionString, DatabaseType databaseType);
		void CreateDatabase(string connectionToNewDb, DatabaseType databaseType, string dbPath, string login, bool isAzure);
		void CreateAzureDatabase(string connectionToNewDb, DatabaseType databaseType, string dbPath, string login);
		void AddSuperUser(string connectionToNewDb, Guid personId, string firstName, string lastName);
		void AddBusinessUnit(string connectionToNewDb, string name);
		bool LoginExists(string connectionToNewDb, string login, bool isAzure);
		void CreateLogin(string connectionToNewDb, string login, string password, bool isAzure);
		bool HasCreateDbPermission(string connectionString, bool isAzure);
		bool HasCreateViewAndLoginPermission(string connectionString, bool isAzure);
		bool LoginCanBeCreated(string connectionString, string login, string password, bool isAzure, out string message);
	}

	public class DatabaseHelperWrapper : IDatabaseHelperWrapper
	{
		public DbCheckResultModel Exists(string databaseConnectionString, DatabaseType databaseType)
		{
			var dbType = "Teleopti WFM application database";
			if (databaseType.Equals(DatabaseType.TeleoptiAnalytics))
				dbType = "Teleopti WFM analytics database";
			try
			{
				new SqlConnectionStringBuilder(databaseConnectionString);
			}
			catch (Exception)
			{
				return new DbCheckResultModel {Exists = false, Message = string.Format("The connection string for {0} is not in the correct format!",dbType)};

			}
			
			var connection = new SqlConnection(databaseConnectionString);
			try
			{
				connection.Open();
			}
			catch (Exception e)
			{
				return new DbCheckResultModel { Exists = false, Message = string.Format("Can not connect to the {0}. " + e.Message, dbType) };
			}

			var helper = new DatabaseHelper(databaseConnectionString, databaseType);
			if(!helper.IsCorrectDb())
				return new DbCheckResultModel { Exists = false, Message = string.Format("The database is not a {0}.", dbType) };

			//later check so it is not used in other Tenants?
			return new DbCheckResultModel {Exists = true, Message =  string.Format("{0} exists.",dbType)};
			
		}

		public void CreateDatabase(string connectionToNewDb, DatabaseType databaseType, string dbPath, string login, bool isAzure)
		{

			if (isAzure && databaseType.Equals(DatabaseType.TeleoptiCCCAgg))
				return;

			try
			{
				new SqlConnectionStringBuilder(connectionToNewDb);
			}
			catch (Exception)
			{
				//return new DbCheckResultModel { Exists = false, Message = string.Format("The connection string for {0} is not in the correct format!") };
				//
			}
			var helper = new DatabaseHelper(connectionToNewDb, databaseType);
			if(helper.Exists())
				return;
			helper.DbManagerFolderPath = dbPath;
			
			if(isAzure)
				helper.CreateInAzureByDbManager();
			else
				helper.CreateByDbManager();

			helper.CreateSchemaByDbManager();
			helper.AddPermissions(login);
		}

		public void CreateAzureDatabase(string connectionToNewDb, DatabaseType databaseType, string dbPath, string login)
		{
			//if it already exists - return a Message!
			if(databaseType.Equals(DatabaseType.TeleoptiCCCAgg))
				return;
			try
         {
				new SqlConnectionStringBuilder(connectionToNewDb);
			}
			catch (Exception)
			{
				//return new DbCheckResultModel { Exists = false, Message = string.Format("The connection string for {0} is not in the correct format!") };
				//
			}
			var helper = new DatabaseHelper(connectionToNewDb, databaseType);
			helper.DbManagerFolderPath = dbPath;
			helper.CreateByDbManager();
			helper.CreateSchemaByDbManager();
			helper.AddPermissions(login);
		}

		public void AddSuperUser(string connectionToNewDb, Guid personId, string firstName, string lastName)
		{
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7);
			helper.AddSuperUser(personId, firstName, lastName);
		}

		public void AddBusinessUnit(string connectionToNewDb, string name)
		{
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7);
			helper.AddBusinessUnit(name);
		}

		public bool LoginExists(string connectionToNewDb, string login, bool isAzure)
		{
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7);
			return(helper.LoginExists(login, isAzure));
		}

        public void CreateLogin(string connectionToNewDb, string login, string password, bool isAzure)
		{
			// type does not mather now
			var helper = new DatabaseHelper(connectionToNewDb, DatabaseType.TeleoptiCCC7);
			helper.CreateLogin(login,password, isAzure);
		}

		public bool HasCreateDbPermission(string connectionString, bool isAzure)
		{
			var helper = new DatabaseHelper(connectionString, DatabaseType.TeleoptiCCC7);
			return helper.HasCreateDbPermission(isAzure);
		}

		public bool HasCreateViewAndLoginPermission(string connectionString, bool isAzure)
		{
			var helper = new DatabaseHelper(connectionString, DatabaseType.TeleoptiCCC7);
			return helper.HasCreateViewAndLoginPermission(isAzure);
		}

		public bool LoginCanBeCreated(string connectionString, string login, string password, bool isAzure, out string message)
		{
			var helper = new DatabaseHelper(connectionString, DatabaseType.TeleoptiCCC7);
			return helper.LoginCanBeCreated(login, password, isAzure, out message);
		}
   }
}