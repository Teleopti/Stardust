using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseSchemeCreator
	{
		private readonly SqlConnection _sqlConnection;
		private readonly DatabaseFolder _databaseFolder;
		private readonly ILog _logger;

		public DatabaseSchemeCreator(SqlConnection sqlConnection, DatabaseFolder databaseFolder, ILog logger)
		{
			_sqlConnection = sqlConnection;
			_databaseFolder = databaseFolder;
			_logger = logger;
		}

		private int GetDatabaseBuildNumber()
		{
			using (SqlCommand sqlCommand = new SqlCommand("SELECT MAX(BuildNumber) FROM dbo.[DatabaseVersion]", _sqlConnection))
			{
				return (int)sqlCommand.ExecuteScalar();
			}
		}

		public void CreateScheme(string databaseTypeName)
		{
			var currentDatabaseBuildNumber = GetDatabaseBuildNumber();
			string releasesPath = _databaseFolder.Path() + @"\" + databaseTypeName + @"\Releases";
			if (Directory.Exists(releasesPath))
			{
				DirectoryInfo scriptsDirectoryInfo = new DirectoryInfo(releasesPath);
				FileInfo[] scriptFiles = scriptsDirectoryInfo.GetFiles("*.sql", SearchOption.TopDirectoryOnly);

				foreach (FileInfo scriptFile in scriptFiles)
				{
					//Check the versionnumber
					string releaseName = scriptFile.Name.Replace(".sql", "");
					int releaseNumber = Convert.ToInt32(releaseName, CultureInfo.CurrentCulture);

					//Always add release files (DDL and src-code)
					if (releaseNumber > currentDatabaseBuildNumber)
					{
						_logger.Write("Applying Release " + releaseName + "...");
						string fileName = scriptFile.FullName;
						string sql;
						using (TextReader textReader = new StreamReader(fileName))
						{
							sql = textReader.ReadToEnd();
						}
						new SQLBatchExecutor(_sqlConnection, _logger).ExecuteBatchSQL(sql);
					}
				}
			}
		}

	}
}
