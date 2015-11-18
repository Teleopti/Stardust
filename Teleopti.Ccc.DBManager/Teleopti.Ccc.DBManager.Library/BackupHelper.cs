using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Teleopti.Ccc.DBManager.Library
{
	public class BackupHelper
	{
		private readonly ExecuteSql _executeSql;
		private readonly ExecuteSql _masterExecuteSql;

		public BackupHelper(ExecuteSql executeSql, ExecuteSql masterExecuteSql, string databaseName)
		{
			DatabaseName = databaseName;
			_executeSql = executeSql;
			_masterExecuteSql = masterExecuteSql;
		}

		public string DatabaseName { get; private set; }
		
		public Backup BackupByFileCopy(string name)
		{
			var backup = new Backup();
			_executeSql.ExecuteCustom(conn =>
			{
				using (var command = conn.CreateCommand())
				{
					command.CommandText = string.Format("select * from sys.sysfiles");
					using (var reader = command.ExecuteReader())
					{
						backup.Files = (from r in reader.Cast<IDataRecord>()
							let file = r.GetString(r.GetOrdinal("filename"))
							select new BackupFile {Source = file})
							.ToArray();
					}
				}
			});
			using (offlineScope())
			{
				backup.Files.ForEach(f =>
				{
					f.Backup = f.Source + "." + name;
					var command = string.Format(@"COPY ""{0}"" ""{1}""", f.Source, f.Backup);
					var result = executeShellCommandOnServer(command);
					if (!result.Contains("1 file(s) copied."))
						throw new Exception();
				});
			}
			return backup;
		}

		public bool TryRestoreByFileCopy(Backup backup)
		{
			using (offlineScope())
			{
				return backup.Files.All(f =>
				{
					var command = string.Format(@"COPY ""{0}"" ""{1}""", f.Backup, f.Source);
					var result = executeShellCommandOnServer(command);
					return result.Contains("1 file(s) copied.");
				});
			}
		}

		public class Backup
		{
			public IEnumerable<BackupFile> Files { get; set; }
		}

		public class BackupFile
		{
			public string Source { get; set; }
			public string Backup { get; set; }
		}

		private IDisposable offlineScope()
		{
			SqlConnection.ClearAllPools();

			var state = new DatabaseTasks(_masterExecuteSql);
			state.SetOffline(DatabaseName);
			return new GenericDisposable(()=>state.SetOnline(DatabaseName));
		}

		private string executeShellCommandOnServer(string command)
		{
			var result = string.Empty;
			_masterExecuteSql.ExecuteCustom(conn =>
			{
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "EXEC sp_configure 'show advanced options', 1";
					cmd.ExecuteNonQuery();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "RECONFIGURE";
					cmd.ExecuteNonQuery();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "EXEC sp_configure 'xp_cmdshell', 1";
					cmd.ExecuteNonQuery();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "RECONFIGURE";
					cmd.ExecuteNonQuery();
				}

				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "xp_cmdshell '" + command + "'";
					using (var reader = cmd.ExecuteReader())
					{
						reader.Read();
						result = reader.GetString(0);
					}
				}
			});
			return result;
		}
	}
}