namespace Teleopti.Support.Tool.Controls.DatabaseDeployment
{
	public static class DatabaseDeployHelper
	{
		public static void MapSuggestions(DatabaseDeploymentModel model)
		{
			model.SelectableDatabaseFiles.ForEach(f =>
				{
					if (f.ToLower().Contains("cccagg"))
						model.SuggestedAggDatabase = new DatabaseInfo
							{
								DatabasePath = f,
								DatabaseSource = BackupFileType.TeleoptiCCCAgg
							};
					else if (f.ToLower().Contains("ccc") && !f.ToLower().Contains("cccagg"))
						model.SuggestedAppDatabase = new DatabaseInfo
							{
								DatabasePath = f,
								DatabaseSource = BackupFileType.TeleoptiCCC7
							};
					else if (f.ToLower().Contains("analytics"))
						model.SuggestedAnalyticsDatabase = new DatabaseInfo
							{
								DatabasePath = f,
								DatabaseSource = BackupFileType.TeleoptiAnalytics
							};
				});
		}
	}
}
