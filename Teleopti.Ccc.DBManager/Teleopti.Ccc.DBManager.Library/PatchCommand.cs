using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Ccc.DBManager.Library
{
	public class PatchCommand
	{
		public static PatchCommand ParseCommandLine(IEnumerable<string> args)
		{
			var command = new PatchCommand();

			foreach (var s in args)
			{
				var @switch = s.Substring(0, 2).ToUpper(CultureInfo.CurrentCulture);
				var value = s.Remove(0, 2);
				switch (@switch)
				{
					case "-S":
						command.ServerName = value;
						break;
					case "-D":
						command.DatabaseName = value;
						break;
					case "-U":
						command.UserName = value;
						break;
					case "-P":
						command.Password = value;
						break;
					case "-E":
						command.UseIntegratedSecurity = true;
						break;
					case "-O":
						command.DatabaseType = (DatabaseType) Enum.Parse(typeof(DatabaseType), value);
						break;
					case "-C":
						command.CreateDatabase = true;
						command.UpgradeDatabase = true;
						break;
					case "-T":
						command.UpgradeDatabase = true;
						break;
					case "-L":
						command.CreatePermissions = true;
						var userAndPassword = value.Split(':');
						if (userAndPassword.Length == 2)
						{
							command.AppUserName = userAndPassword[0];
							command.AppUserPassword = userAndPassword[1];
							command.IsWindowsGroupName = false;
						}
						else
						{
							throw new Exception("Not the correct inparameters for application sql user and password");
						}

						break;
					case "-W":
						command.CreatePermissions = true;
						command.AppUserName = value;
						command.IsWindowsGroupName = true;
						break;
					case "-F":
						command.DbManagerFolderPath = value;
						break;
					case "-R":
						command.CreatePermissions = true;
						break;
				}
			}

			return command;
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
	}
}