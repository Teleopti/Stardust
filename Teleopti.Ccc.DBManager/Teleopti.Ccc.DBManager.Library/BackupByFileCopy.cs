using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Support.Library;

namespace Teleopti.Ccc.DBManager.Library
{
	public class BackupByFileCopy
	{
		private readonly ExecuteSql _usingDatabase;
		private readonly ExecuteSql _usingMaster;
		private readonly Offline _offline;

		public BackupByFileCopy(ExecuteSql usingDatabase, ExecuteSql usingMaster, string databaseName)
		{
			DatabaseName = databaseName;
			_usingDatabase = usingDatabase;
			_usingMaster = usingMaster;
			_offline = new Offline(_usingMaster, DatabaseName);
		}

		public string DatabaseName { get; }
		
		public Backup Backup(string name)
		{
			var backup = new Backup();
			_usingDatabase.Execute(conn =>
			{
				using (var command = conn.CreateCommand())
				{
					command.CommandText = "select * from sys.sysfiles";
					using (var reader = command.ExecuteReader())
					{
						backup.Files = (from r in reader.Cast<IDataRecord>()
							let file = r.GetString(r.GetOrdinal("filename"))
							select new BackupFile {Source = file})
							.ToArray();
					}
				}
			});
			using (_offline.OfflineScope())
			{
				backup.Files.ForEach(f =>
				{
					f.Backup = f.Source + "." + name;
					var command = $@"COPY ""{f.Source}"" ""{f.Backup}""";
					var result = executeShellCommandOnServer(command);
					if (!result.Contains("1 file(s) copied."))
						throw new Exception();
				});
			}
			return backup;
		}

		public bool TryRestore(Backup backup)
		{
			using (_offline.OfflineScope())
			{
				return backup.Files.All(f =>
				{
					var command = $@"COPY ""{f.Backup}"" ""{f.Source}""";
					var result = executeShellCommandOnServer(command);
					return result.Contains("1 file(s) copied.");
				});
			}
		}

		private string executeShellCommandOnServer(string command)
		{
			var result = string.Empty;
			_usingMaster.Execute(conn =>
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

	public class Backup
	{
		public IEnumerable<BackupFile> Files { get; set; }
	}

	public class BackupFile
	{
		public string Source { get; set; }
		public string Backup { get; set; }
	}

}