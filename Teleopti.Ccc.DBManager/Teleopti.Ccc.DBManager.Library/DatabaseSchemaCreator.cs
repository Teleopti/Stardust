using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseSchemaCreator
	{
		private readonly DatabaseVersionInformation _versionInformation;
		private readonly SqlConnection _sqlConnection;
		private readonly DatabaseFolder _databaseFolder;
		private readonly ILog _logger;

		public DatabaseSchemaCreator(DatabaseVersionInformation versionInformation, SqlConnection sqlConnection, DatabaseFolder databaseFolder, ILog logger)
		{
			_versionInformation = versionInformation;
			_sqlConnection = sqlConnection;
			_databaseFolder = databaseFolder;
			_logger = logger;
		}

		public void CreateSchema(DatabaseType databaseType)
		{
			var currentDatabaseBuildNumber = _versionInformation.GetDatabaseBuildNumber();
			var releasesPath = _databaseFolder.ReleasePath(databaseType);
			
			if (!Directory.Exists(releasesPath)) return;
			
			var scriptsDirectoryInfo = new DirectoryInfo(releasesPath);
			var scriptFiles = scriptsDirectoryInfo.GetFiles("*.sql", SearchOption.TopDirectoryOnly);

			var applicableScriptFiles = from f in scriptFiles
			                            let name = f.Name.Replace(".sql", "")
			                            let number = Convert.ToInt32(name)
			                            where number > currentDatabaseBuildNumber
										orderby number
			                            select new {file = f, name};

			foreach (var scriptFile in applicableScriptFiles)
			{
				_logger.Write("Applying Release " + scriptFile.name + "...");
				var sql = File.ReadAllText(scriptFile.file.FullName);
				new SqlBatchExecutor(_sqlConnection, _logger).ExecuteBatchSql(sql);
			}
		}

	}
}
