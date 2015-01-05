namespace AnalysisServicesManager
{
	public class FolderInformation
	{
		public string CurrentDir { get; set; }
		public string FilePath { get; set; }
		public string CustomFilePath { get; set; }

		public FolderInformation(string currentDir, string filePath, string customFilePath)
		{
			CurrentDir = currentDir;
			FilePath = filePath;
			CustomFilePath = customFilePath;
		}
	}
}