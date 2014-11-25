using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseSchemaCreator
	{
		private readonly DatabaseVersionInformation _versionInformation;
		private readonly SchemaVersionInformation _schemaVersionInformation;
		private readonly SqlConnection _sqlConnection;
		private readonly DatabaseFolder _databaseFolder;
		private readonly ILog _logger;

		public DatabaseSchemaCreator(DatabaseVersionInformation versionInformation, SchemaVersionInformation schemaVersionInformation, SqlConnection sqlConnection, DatabaseFolder databaseFolder, ILog logger)
		{
			_versionInformation = versionInformation;
			_schemaVersionInformation = schemaVersionInformation;
			_sqlConnection = sqlConnection;
			_databaseFolder = databaseFolder;
			_logger = logger;
		}

		public void Create(DatabaseType databaseType)
		{
			applyReleases(databaseType);
			applyTrunk(databaseType);
			applyProgrammability(databaseType);
		}

		private void applyReleases(DatabaseType databaseType)
		{
			var currentDatabaseBuildNumber = _versionInformation.GetDatabaseVersion();
			var releasesPath = _databaseFolder.ReleasePath(databaseType);
			
			if (!Directory.Exists(releasesPath)) return;
			
			var scriptsDirectoryInfo = new DirectoryInfo(releasesPath);
			var scriptFiles = scriptsDirectoryInfo.GetFiles("*.sql", SearchOption.TopDirectoryOnly);

			var applicableScriptFiles = from f in scriptFiles
										let number = _schemaVersionInformation.ReleaseNumberOfFile(f)
			                            where number > currentDatabaseBuildNumber
										orderby number
			                            select new {file = f, number};

			foreach (var scriptFile in applicableScriptFiles)
			{
				try
				{
					_logger.Write("Applying Release " + scriptFile.number + "...");
					var sql = File.ReadAllText(scriptFile.file.FullName);
					new SqlBatchExecutor(_sqlConnection, _logger)
						.ExecuteBatchSql(sql);
				}
				catch (SqlException exception)
				{
					throw new NotExecutableScriptException(scriptFile.file.FullName, "scriptFile", exception);
				}
			}
		}

		private void applyTrunk(DatabaseType databaseType)
		{
			_logger.Write("Applying Trunk...");
			var trunkPath = _databaseFolder.TrunkPath(databaseType);

			if (!Directory.Exists(trunkPath)) return;

			var trunkFile = Path.Combine(trunkPath, "Trunk.sql");
			var sql = File.ReadAllText(trunkFile);

			new SqlBatchExecutor(_sqlConnection, _logger)
				.ExecuteBatchSql(sql);
		}

		private void applyProgrammability(DatabaseType databaseType)
		{
			var programmabilityPath = _databaseFolder.ProgrammabilityPath(databaseType);
			var directories = Directory.GetDirectories(programmabilityPath);

			foreach (var directory in directories)
			{
				var scriptsDirectoryInfo = new DirectoryInfo(directory);
				var scriptFiles = scriptsDirectoryInfo.GetFiles("*.sql", SearchOption.TopDirectoryOnly);

				_logger.Write(string.Format("Applying programmability directory '{0}'", scriptsDirectoryInfo.Name));

				foreach (var scriptFile in scriptFiles)
				{
					var sql = File.ReadAllText(scriptFile.FullName);
					new SqlBatchExecutor(_sqlConnection, _logger)
						.ExecuteBatchSql(sql);
				}
			}
		}

	}
}
