using System;
using System.Data.SqlClient;
using System.IO;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.TestCommon
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class DatabaseHelper : IDisposable
	{
		private readonly string _connectionString;
		private readonly string _databaseName;
		private readonly DatabaseType _databaseType;
		private SqlConnection _connection;

		public DatabaseHelper(string connectionString, DatabaseType databaseType) : this(connectionString, new SqlConnectionStringBuilder(connectionString).InitialCatalog, databaseType) { }
		public DatabaseHelper(string connectionString, string databaseName, DatabaseType databaseType)
		{
			_connectionString = connectionString;
			_databaseName = databaseName;
			_databaseType = databaseType;
		}

		private SqlConnection Connection()
		{
			if (_connection == null)
			{
				var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
				connectionStringBuilder.InitialCatalog = "master";
				_connection = new SqlConnection(connectionStringBuilder.ConnectionString);
				_connection.Open();
				if (Exists())
					ExecuteNonQuery("USE [{0}]", _databaseName);
			}
			return _connection;
		}

		public bool Exists()
		{
			var databaseId = ExecuteScalar<int>("SELECT database_id FROM sys.databases WHERE Name = '{0}'", 0, _databaseName);
			return databaseId > 0;
		}

		public void Drop()
		{
			ExecuteNonQuery("USE master");
			ExecuteNonQuery("DROP DATABASE [{0}]", _databaseName);
		}

		public void Create()
		{
			ExecuteNonQuery("CREATE DATABASE [{0}]", _databaseName);
			ExecuteNonQuery("USE [{0}]", _databaseName);
		}

		public void CreateByDbManager()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder());
			var creator = new DatabaseCreator(databaseFolder, _connection);
			creator.CreateDatabase(_databaseType, _databaseName);
			ExecuteNonQuery("USE [{0}]", _databaseName);
		}

		public void CleanByGenericProcedure()
		{
			ExecuteNonQuery(File.ReadAllText("Teleopti.Ccc.TestCommon.sp_deleteAllData.DROP.sql"));
			ExecuteNonQuery(File.ReadAllText("Teleopti.Ccc.TestCommon.sp_deleteAllData.sql"));
			ExecuteNonQuery("EXEC sp_deleteAllData");
		}

		public void CleanByAnalyticsProcedure()
		{
			ExecuteNonQuery("EXEC [mart].[etl_data_mart_delete] @DeleteAll=1");
		}

		public void DropConnections()
		{
			using (UseMasterScope())
			{
				ExecuteNonQuery("ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE", _databaseName);
				ExecuteNonQuery("ALTER DATABASE [{0}] SET ONLINE", _databaseName);
			}
		}

		public void CreateSchemaByDbManager()
		{
			var databaseFolder = new DatabaseFolder(new DbManagerFolder());
			var versionInfo = new DatabaseVersionInformation(databaseFolder, _connection);
			versionInfo.CreateTable();
			var schemaCreator = new DatabaseSchemaCreator(versionInfo, _connection, databaseFolder, new NullLog());
			schemaCreator.Create(_databaseType);
		}

		//public void Backup()
		//{
		//    using (UseMasterScope())
		//    {
		//        ExecuteNonQuery("ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE", _databaseName);
		//        File.Copy(@"C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\RepositoryTest_Data.mdf", @"C:\1.mdf", true);
		//        File.Copy(@"C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\RepositoryTest_Log.ldf", @"C:\1.ldf", true);
		//        ExecuteNonQuery("ALTER DATABASE [{0}] SET ONLINE", _databaseName);
		//    }
		//    //ExecuteNonQuery(@"BACKUP DATABASE [{0}] TO DISK='{0}.bak' WITH INIT, STATS=10", _databaseName);
		//}

		//public void Restore()
		//{
		//    using (UseMasterScope())
		//    {
		//        using (PerformanceOutput.ForOperation("offline"))
		//            ExecuteNonQuery("ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE", _databaseName);
		//        using (PerformanceOutput.ForOperation("copy"))
		//            File.Copy(@"C:\1.mdf", @"C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\RepositoryTest_Data.mdf", true);
		//        using (PerformanceOutput.ForOperation("copy"))
		//            File.Copy(@"C:\1.ldf", @"C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\RepositoryTest_Log.ldf", true);
		//        using (PerformanceOutput.ForOperation("online"))
		//            ExecuteNonQuery("ALTER DATABASE [{0}] SET ONLINE", _databaseName);
		//        //ExecuteNonQuery(@"RESTORE DATABASE [{0}] FROM DISK='{0}.bak' WITH REPLACE, RECOVERY, STATS=10", _databaseName);
		//    }
		//}

		public IDisposable UseMasterScope()
		{
			ExecuteNonQuery("USE master");
			return new GenericDisposable(() => ExecuteNonQuery("USE [{0}]", _databaseName));
		}



		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private void ExecuteNonQuery(string sql, params object[] args)
		{
			using (var command = Connection().CreateCommand())
			{
				command.CommandText = string.Format(sql, args);
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private T ExecuteScalar<T>(string sql, T nullValue, params object[] args)
		{
			using (var command = Connection().CreateCommand())
			{
				command.CommandText = string.Format(sql, args);
				var value = command.ExecuteScalar();
				if (value == null)
					return nullValue;
				return (T) value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		public void Dispose()
		{
			if (_connection != null)
				_connection.Dispose();
		}


	}


	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class GenericDisposable : IDisposable
	{
		private Action _disposeAction;

		public GenericDisposable(Action disposeAction)
		{
			_disposeAction = disposeAction;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		public void Dispose()
		{
			_disposeAction.Invoke();
			_disposeAction = null;
		}
	}

}