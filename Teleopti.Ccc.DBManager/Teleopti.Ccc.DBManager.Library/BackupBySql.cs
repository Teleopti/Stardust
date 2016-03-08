using System;
using System.IO;

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
			var fileName = name + ".bak";
			var target = Path.Combine(path, fileName);
			var localTarget = Path.Combine(sqlBackupPath(), fileName);
			_usingMaster.Execute(string.Format(@"BACKUP DATABASE {0} TO DISK = '{1}' WITH FORMAT", _databaseName, localTarget));
			File.Copy(localTarget, target);
			File.Delete(localTarget);
		}

		public bool TryRestore(string path, string name)
		{
			var fileName = name + ".bak";
			var source = Path.Combine(path, fileName);
			if (!File.Exists(source))
				return false;
			var localSource = Path.Combine(sqlBackupPath(), fileName);
			File.Copy(source, localSource, true);
			_usingMaster.Execute(string.Format(@"RESTORE DATABASE {0} FROM DISK = '{1}'", _databaseName, localSource));
			File.Delete(localSource);
			return true;
		}

		private string sqlBackupPath()
		{
			return Microsoft.Win32.Registry.LocalMachine
				.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQLServer")
				.GetValue("BackupDirectory") as string;
		}
	}
}