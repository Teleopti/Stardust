using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Teleopti.Support.Library;

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
					x => matchSwitch(x, "-E", () => command.UseIntegratedSecurity = true),
					x => matchSwitch(x, "-C", () =>
					{
						command.CreateDatabase = true;
						command.UpgradeDatabase = true;
					}),
					x => matchSwitch(x, "-T", "-TRUNK", () => command.UpgradeDatabase = true),
					x => matchSwitch(x, "-R", () => command.CreatePermissions = true),


					x => matchSwitchWithAdjacentValue(x, "-RESTORETRUNK", v =>
					{
						command.RestoreBackupIfNotExistsOrNewer = v;
						command.UpgradeDatabase = true;
					}),
					x => matchSwitchWithAdjacentValue(x, "-RESTORE", v => command.RestoreBackup = v),


					x => matchSwitchWithAdjacentValue(x, "-S", v => command.ServerName = v),
					x => matchSwitchWithAdjacentValue(x, "-D", v => command.DatabaseName = v),
					x => matchSwitchWithAdjacentValue(x, "-U", v => command.UserName = v),
					x => matchSwitchWithAdjacentValue(x, "-P", v => command.Password = v),
					x => matchSwitchWithAdjacentValue(x, "-O", v => command.DatabaseType = (DatabaseType) Enum.Parse(typeof(DatabaseType), v)),
					x => matchSwitchWithAdjacentValue(x, "-L", v =>
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
					x => matchSwitchWithAdjacentValue(x, "-W", v =>
					{
						command.AppUserName = v;
						command.CreatePermissions = true;
						command.IsWindowsGroupName = true;
					}),
					x => matchSwitchWithAdjacentValue(x, "-F", v => command.DbManagerFolderPath = v),
				}.Any(matcher => matcher(s));
			}

			return command;
		}

		private static bool matchSwitch(string arg, string match1, Action found) =>
			matchSwitch(arg, match1, null, found);

		private static bool matchSwitch(string arg, string match1, string match2, Action found)
		{
			var result = new[] {match1, match2}
				.Where(x => x != null)
				.Any(x => arg.ToUpper(CultureInfo.CurrentCulture).Equals(x));
			if (result)
				found();
			return result;
		}

		private static bool matchSwitchWithAdjacentValue(string arg, string match1, Action<string> value) =>
			matchSwitchWithAdjacentValue(arg, match1, null, value);

		private static bool matchSwitchWithAdjacentValue(string arg, string match1, string match2, Action<string> value)
		{
			var match = new[] {match1, match2}
				.Where(x => x != null)
				.OrderByDescending(x => x.Length)
				.FirstOrDefault(arg.StartsWith);

			if (match == null)
				return false;

			var v = arg.Remove(0, match.Length);
			value(v);
			return true;
		}


		public string DbManagerFolderPath { get; set; }

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

		public bool DropDatabase { get; set; }
		public bool CreateDatabase { get; set; }
		public bool RecreateDatabaseIfNotExistsOrNewer { get; set; }

		public string RestoreBackup { get; set; }
		public string RestoreBackupIfNotExistsOrNewer { get; set; }

		public bool CreatePermissions { get; set; }
		public bool UpgradeDatabase { get; set; }
	}
}