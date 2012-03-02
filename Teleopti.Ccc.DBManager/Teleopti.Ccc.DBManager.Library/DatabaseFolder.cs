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