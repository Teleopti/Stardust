namespace Teleopti.Support.Tool.Controls.DatabaseDeployment
{
	public static class DatabaseDeployHelper
	{
		public static void MapSuggestions(DatabaseDeploymentModel model)
		{
			model.SelectableDatabaseFiles.ForEach(f =>
			{
				var file = f.ToLower();
				if (file.Contains("cccagg"))
						model.SuggestedAggDatabase = new DatabaseInfo
							{
								DatabasePath = f,
								DatabaseSource = BackupFileType.TeleoptiCCCAgg
							};
					else if (file.Contains("ccc") && !file.Contains("cccagg"))
						model.SuggestedAppDatabase = new DatabaseInfo
							{
								DatabasePath = f,
								DatabaseSource = BackupFileType.TeleoptiCCC7
							};
					else if (file.Contains("analytics"))
						model.SuggestedAnalyticsDatabase = new DatabaseInfo
							{
								DatabasePath = f,
								DatabaseSource = BackupFileType.TeleoptiAnalytics
							};
			});
		}
	}
}
