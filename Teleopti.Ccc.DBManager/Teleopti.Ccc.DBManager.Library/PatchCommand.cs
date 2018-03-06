using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Teleopti.Ccc.DBManager.Library
{
	public class PatchCommand
	{
		public static PatchCommand ParseCommandLine(IEnumerable<string> args)
		{
			var command = new PatchCommand();

			foreach (var s in args)
			{
				new Func<string, bool>[]
				{
					x => matchArgument(x, "-S", v => command.ServerName = v),
					x => matchArgument(x, "-D", v => command.DatabaseName = v),
					x => matchArgument(x, "-U", v => command.UserName = v),
					x => matchArgument(x, "-P", v => command.Password = v),
					x => matchArgument(x, "-E", v => command.UseIntegratedSecurity = true),
					x => matchArgument(x, "-O", v => command.DatabaseType = (DatabaseType) Enum.Parse(typeof(DatabaseType), v)),
					x => matchArgument(x, "-C", v =>
					{
						command.CreateDatabase = true;
						command.UpgradeDatabase = true;
					}),
					x => matchArgument(x, "-T", v => command.UpgradeDatabase = true),
					x => matchArgument(x, "-L", v =>
					{
						command.CreatePermissions = true;
						var userAndPassword = v.Split(':');
						if (userAndPassword.Length == 2)
						{
							command.AppUserName = userAndPassword[0];
							command.AppUserPassword = userAndPassword[1];
							command.IsWindowsGroupName = false;
						}
						else
							throw new Exception("Not the correct inparameters for application sql user and password");
					}),
					x => matchArgument(x, "-W", v =>
					{
						command.CreatePermissions = true;
						command.AppUserName = v;
						command.IsWindowsGroupName = true;
					}),
					x => matchArgument(x, "-F", v => command.DbManagerFolderPath = v),
					x => matchArgument(x, "-R", v => command.CreatePermissions = true),
					x => matchArgument(x, "-🏄🐘", v =>
					{
						command.RestoreBackupIfNotExistsOrNewer = v;
						command.UpgradeDatabase = true;
					}),
					x => matchArgument(x, "-🏄", v => command.RestoreBackup = v),
					x => matchArgument(x, "-🐘", v => command.UpgradeDatabase = true),
				}.Any(matcher => matcher(s));
			}

			return command;
		}
		
		private static bool matchArgument(string arg, string match, Action<string> value)
		{
			if (arg.ToUpper(CultureInfo.CurrentCulture).StartsWith(match))
			{
				var v = arg.Remove(0, match.Length);
				value(v);
				return true;
			}

			return false;
		}

		public string DbManagerFolderPath { get; set; }
		public bool IsAzure => ServerName.Contains(".database.windows.net");
		public string ServerName { get; set; } = ".";
		public string DatabaseName { get; set; }
		public string UserName { get; set; }
		public string AppUserName { get; set; } = "";
		public string AppUserPassword { get; set; } = "";
		public bool IsWindowsGroupName { get; private set; }
		public string Password { get; set; }
		public bool UseIntegratedSecurity { get; set; }
		public DatabaseType DatabaseType { get; set; }
		public string DatabaseTypeName => DatabaseType.GetName();
		public bool CreateDatabase { get; private set; }
		public bool CreatePermissions { get; set; }
		public bool UpgradeDatabase { get; set; }
		public string RestoreBackup { get; private set; }
		public string RestoreBackupIfNotExistsOrNewer { get; set; }
	}
}