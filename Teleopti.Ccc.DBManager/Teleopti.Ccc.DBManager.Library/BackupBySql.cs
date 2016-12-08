using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

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

			var sql = string.Format(@"BACKUP DATABASE {0} TO DISK = '{1}' WITH FORMAT", _databaseName, localTarget);
			_usingMaster.Execute(sql);

			File.Copy(localTarget, target, true);
			File.Delete(localTarget);
		}

		public bool TryRestore(string path, string name)
		{
			Console.WriteLine("TryRestore");
			var fileName = name + ".bak";
			var source = Path.Combine(path, fileName);
			if (!File.Exists(source))
			{
				Console.WriteLine("/TryRestore false");
				return false;
			}
			var localSource = Path.Combine(sqlBackupPath(), fileName);
			File.Copy(source, localSource, true);

			var tasks = new DatabaseTasks(_usingMaster, null);
			Console.WriteLine("TryRestore Drop");
			tasks.Drop(_databaseName);
			Console.WriteLine("TryRestore /Drop");
			Console.WriteLine("TryRestore RESORWE");
			_usingMaster.Execute($@"RESTORE DATABASE {_databaseName} FROM DISK = '{localSource}' WITH REPLACE");
			Console.WriteLine("TryRestore /RESORWE");

			File.Delete(localSource);
			Console.WriteLine("/TryRestore true");
			return true;
		}

		private string sqlBackupPath()
		{
			// A 32-bit application on a 64-bit OS will be looking at the HKLM\Software\Wow6432Node node by default. 
			//
			// BUILDAGENT03: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQLServer
			// BUILDAGENT03: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL12.SQL2014\MSSQLServer
			// TELEOPTI710: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQLServer
			//
			var locations = new[]
			{
				@"SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQLServer",
				@"SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQLServer",
				@"SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL12.SQL2014\MSSQLServer"
			};
			var roots = new[]
			{
				RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64),
				RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
			};
			var paths = from r in roots
				from l in locations
				let key = r.OpenSubKey(l)
				where key != null
				select key.GetValue("BackupDirectory") as string;
			return paths.FirstOrDefault();
		}
	}
}