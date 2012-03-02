using System;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseFolder
	{
		private readonly DbManagerFolder _dbManagerFolder;

		public DatabaseFolder(DbManagerFolder dbManagerFolder)
		{
			_dbManagerFolder = dbManagerFolder;
		}

		public string Path()
		{
#if DEBUG
			var path = LocateDatabaseFolderUsingBlackMagic();
#else
			var path = _dbManagerFolder.Path();
#endif
			return System.IO.Path.GetFullPath(path);
		}

		public string CreateScriptsPath()
		{
			return System.IO.Path.Combine(Path(), "Create");
		}

		public string AzureCreateScriptsPath()
		{
			return System.IO.Path.Combine(CreateScriptsPath(), "Azure");
		}

		private string LocateDatabaseFolderUsingBlackMagic()
		{
			if (System.IO.Directory.Exists(@"..\..\..\..\Database"))
				return @"..\..\..\..\Database";
			if (System.IO.Directory.Exists(@"..\..\..\Database"))
				return @"..\..\..\Database";
			if (System.IO.Directory.Exists(@"..\..\Database"))
				return @"..\..\Database";
			throw new Exception("LocateDatabaseFolderUsingBlackMagic failed!");
		}

		public override string ToString()
		{
			return Path();
		}
	}
}