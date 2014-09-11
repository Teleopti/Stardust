using System.Collections.Generic;
using Teleopti.Support.Tool.DataLayer;

namespace Teleopti.Support.Tool.Controls.DatabaseDeployment
{
	public class DatabaseDeploymentModel
	{
		public DatabaseDeploymentModel()
		{
			SelectableDatabaseFiles = new List<string>();
			SelectedAppDatabase = new DatabaseInfo();
			SelectedAnalyticsDatabase = new DatabaseInfo();
			SelectedAggDatabase = new DatabaseInfo();
			UnzipPath = @"C:\Temp\TeleoptiSupportTool\";
		}

		public DBHelper Helper { get; set; }
        public string SevenZipDllUrn { get; set; }
		public List<string> SelectableDatabaseFiles { get; private set; }

		public DatabaseInfo SelectedAppDatabase { get; set; }
		public DatabaseInfo SelectedAnalyticsDatabase { get; set; }
		public DatabaseInfo SelectedAggDatabase { get; set; }

		public DatabaseInfo SuggestedAppDatabase { get; set; }
		public DatabaseInfo SuggestedAnalyticsDatabase { get; set; }
		public DatabaseInfo SuggestedAggDatabase { get; set; }

		public string ZipFilePath { get; set; }
		public string UnzipPath { get; set; }
		public bool SkippedFirstStep { get; set; }

		public string SessionName { get; set; }
		public string NHibPath { get; set; }

		public List<DatabaseInfo> GetSuggestions()
		{
			return new List<DatabaseInfo> {SuggestedAppDatabase, SuggestedAnalyticsDatabase, SuggestedAggDatabase};
		}

		public List<DatabaseInfo> GetSelections()
		{
			return new List<DatabaseInfo> {SelectedAppDatabase, SelectedAnalyticsDatabase, SelectedAggDatabase};
		}
	}

	public class DatabaseInfo
	{
		public string DatabaseName;
		public string DatabasePath;
		public BackupFileType DatabaseSource;
		public DatabaseSourceType DatabaseFromSourceType;
	}

	public enum BackupFileType
	{
		TeleoptiCCC7,
		TeleoptiAnalytics,
		TeleoptiCCCAgg
	}
	
	public enum DatabaseSourceType
	{
		FromArchive,
		ExistingFile,
		ExistingDatabase
	}
}
