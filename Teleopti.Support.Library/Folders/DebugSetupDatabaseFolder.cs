namespace Teleopti.Support.Library.Folders
{
	public class DebugSetupDatabaseFolder
	{
		private readonly RepositoryRootFolder _repositoryRootFolder;

		public DebugSetupDatabaseFolder(RepositoryRootFolder repositoryRootFolder)
		{
			_repositoryRootFolder = repositoryRootFolder;
		}

		public string Path()
		{
			var path = System.IO.Path.Combine(_repositoryRootFolder.Path(), @".debug-Setup\database");
			return System.IO.Path.GetFullPath(path);
		}

		public override string ToString() => Path();
	}
}