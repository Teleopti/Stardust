using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseSchemaCreator
	{
		private readonly DatabaseVersionInformation _versionInformation;
		private readonly SchemaVersionInformation _schemaVersionInformation;
		private readonly SqlConnection _sqlConnection;
		private readonly DatabaseFolder _databaseFolder;
		private readonly IUpgradeLog _logger;
		private const int buildNumberWhenTrunkDisappeared = 500;

		public DatabaseSchemaCreator(DatabaseVersionInformation versionInformation, SchemaVersionInformation schemaVersionInformation, SqlConnection sqlConnection, DatabaseFolder databaseFolder, IUpgradeLog logger)
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
			applyProgrammability(databaseType);
			if (databaseType == DatabaseType.TeleoptiAnalytics)
				new HangfireSchemaCreator().ApplyHangfire(_sqlConnection);
			addInstallLogRow();
		}

		private void addInstallLogRow()
		{
			const string sql = "insert into databaseversion_installlog (databaseversion, codeversion) values ({0}, '{1}')";
			var latestDatabaseBuildNumber = _versionInformation.GetDatabaseVersion();
			new SqlBatchExecutor(_sqlConnection, _logger)
				.ExecuteBatchSql(string.Format(sql, latestDatabaseBuildNumber, codeVersion()));
		}

		private string codeVersion()
		{
			var codeVersion = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(GetType()).Location).ProductVersion;
			if (codeVersion.TrimStart().StartsWith("1"))
				codeVersion = "[develop install]";
			return codeVersion;
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
					if (scriptFile.number >= buildNumberWhenTrunkDisappeared)
					{
						const string dbVersionSql = "INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES ({0},'{1}')";
						new SqlBatchExecutor(_sqlConnection, _logger)
							.ExecuteBatchSql(string.Format(dbVersionSql, scriptFile.number, codeVersion()));
					}
				}
				catch (SqlException exception)
				{
					throw new NotExecutableScriptException(scriptFile.file.FullName, "scriptFile", exception);
				}
			}
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
