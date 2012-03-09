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
			var path = @"..\..\..\..\Database";
#else
			var path = _dbManagerFolder.Path();
#endif
			return System.IO.Path.GetFullPath(path);
		}

		public override string ToString()
		{
			return Path();
		}
	}
}