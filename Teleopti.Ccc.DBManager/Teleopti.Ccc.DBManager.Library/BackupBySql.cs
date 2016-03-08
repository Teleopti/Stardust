namespace Teleopti.Ccc.DBManager.Library
{
	public class BackupBySql
	{
		private readonly string _databaseName;
		private readonly ExecuteSql _usingMaster;

		public BackupBySql(ExecuteSql usingMaster, string databaseName)
		{
			_usingMaster = usingMaster;
			_databaseName = databaseName;
		}

		public void Backup(string path, string name)
		{
			var file = System.IO.Path.Combine(path, name) + ".bak";
			_usingMaster.ExecuteNonQuery(string.Format(@"BACKUP DATABASE {0} TO DISK = '{1}' WITH FORMAT", _databaseName, file));
		}

		public bool TryRestore(string path, string name)
		{
			var file = System.IO.Path.Combine(path, name) + ".bak";
			if (!System.IO.File.Exists(file))
				return false;
			_usingMaster.ExecuteNonQuery(string.Format(@"RESTORE DATABASE {0} FROM DISK = '{1}'", _databaseName, file));
			return true;
		}
	}
}