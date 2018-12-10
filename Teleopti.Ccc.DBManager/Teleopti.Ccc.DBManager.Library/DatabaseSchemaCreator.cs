using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.Azure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Support.Library;
using Teleopti.Support.Library.Folders;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseSchemaCreator
	{
		private readonly DatabaseVersionInformation _versionInformation;
		private readonly SchemaVersionInformation _schemaVersionInformation;
		private readonly ExecuteSql _executeSql;
		private readonly DatabaseFolder _databaseFolder;
		private readonly IUpgradeLog _logger;
		private const int buildNumberWhenTrunkDisappeared = 500;

		public DatabaseSchemaCreator(DatabaseVersionInformation versionInformation, SchemaVersionInformation schemaVersionInformation, ExecuteSql executeSql, DatabaseFolder databaseFolder, IUpgradeLog logger)
		{
			_versionInformation = versionInformation;
			_schemaVersionInformation = schemaVersionInformation;
			_executeSql = executeSql;
			_databaseFolder = databaseFolder;
			_logger = logger;
		}

		public void Create(DatabaseType databaseType)
		{
			applyReleases(databaseType);
			applyProgrammability(databaseType);
			if (databaseType == DatabaseType.TeleoptiAnalytics)
			{
				_executeSql.Execute(c => new HangfireSchemaCreator().ApplyHangfire(c));
				if(!AzureCommon.IsAzure)
					_executeSql.Execute(c => new SignalRSqlBackplaneSchemaCreator().ApplySignalRSqlBackplane(c));
			}
			addInstallLogRow();
		}

		private void addInstallLogRow()
		{
			const string sql = "insert into databaseversion_installlog (databaseversion, codeversion) values (@dbversion, @codeversion)";
			var latestDatabaseBuildNumber = _versionInformation.GetDatabaseVersion();
			_executeSql.ExecuteNonQuery(sql, Timeouts.CommandTimeout, new Dictionary<string, object>{{"@dbversion", latestDatabaseBuildNumber}, { "@codeversion", codeVersion()}});
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

			string dbVersionSql =
				$@"
{Environment.NewLine}GO
{Environment.NewLine}if not exists (select 1 from DatabaseVersion where BuildNumber = @buildnumber and SystemVersion = @systemversion)
{Environment.NewLine}BEGIN
{Environment.NewLine}INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (@buildnumber,@systemversion)
{Environment.NewLine}END";
						
			foreach (var scriptFile in applicableScriptFiles)
			{
				try
				{
					_logger.Write($"Applying Release {scriptFile.number}...");
					var sql = File.ReadAllText(scriptFile.file.FullName);
					if (scriptFile.number >= buildNumberWhenTrunkDisappeared)
					{
						_executeSql.ExecuteNonQuery(sql + dbVersionSql, Timeouts.CommandTimeout,
							new Dictionary<string, object> {{"@buildnumber", scriptFile.number}, {"@systemversion", codeVersion()}});
					}
					else
					{
						_executeSql.ExecuteNonQuery(sql, Timeouts.CommandTimeout);
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

				_logger.Write($"Applying programmability directory '{scriptsDirectoryInfo.Name}'");

				foreach (var scriptFile in scriptFiles)
				{
					var sql = File.ReadAllText(scriptFile.FullName);
					_executeSql.ExecuteNonQuery(sql,Timeouts.CommandTimeout);
				}
			}
		}
	}
}
