using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Ccc.DBManager.Library
{
	public class CommandLineArgument
	{
		private const string applicationName = "Teleopti.Ccc.DBManager";
		private const string currentLanguage = "us_english";

		public CommandLineArgument(IEnumerable<string> args)
		{
			foreach (var s in args)
			{
				var switchType = s.Substring(0, 2).ToUpper(CultureInfo.CurrentCulture);
				var switchValue = s.Remove(0, 2);
				switch (switchType)
				{
					case "-S":
						ServerName = switchValue;
						break;
					case "-D":
						DatabaseName = switchValue;
						break;
					case "-U":
						UserName = switchValue;
						break;
					case "-P":
						Password = switchValue;
						break;
					case "-E":
						UseIntegratedSecurity = true;
						break;
					case "-O":
						DatabaseType = (DatabaseType) Enum.Parse(typeof(DatabaseType), switchValue);
						break;
					case "-C":
						CreateDatabase = true;
						UpgradeDatabase = true;
						break;
					case "-T":
						UpgradeDatabase = true;
						break;
					case "-L":
						CreatePermissions = true;
						var userAndPassword = switchValue.Split(':');
						if (userAndPassword.Length == 2)
						{
							AppUserName = userAndPassword[0];
							AppUserPassword = userAndPassword[1];
							IsWindowsGroupName = false;
						}
						else
						{
							throw new Exception("Not the correct inparameters for application sql user and password");
						}
						break;
					case "-W":
						CreatePermissions = true;
						AppUserName = switchValue;
						IsWindowsGroupName = true;
						break;
					case "-F":
						DbManagerFolderPath = switchValue;
						break;
					case "-R":
						CreatePermissions = true;
						break;
				}
			}
		}

		public string DbManagerFolderPath { get; set; }
		public bool IsAzure => ServerName.Contains(".database.windows.net");
		public string ServerName { get; set; } = ".";
		public string DatabaseName { get; set; }
		public string UserName { get; set; }
		public string AppUserName { get; set; } = "";
		public string AppUserPassword { get; set; } = "";
		public bool IsWindowsGroupName { get; }
		public string Password { private get; set; }
		public bool UseIntegratedSecurity { get; set; }
		public DatabaseType DatabaseType { get; set; }
		public string TargetDatabaseTypeName => DatabaseType.GetName();
		public bool CreateDatabase { get; }
		public bool CreatePermissions { get; set; }
		public bool UpgradeDatabase { get; set; }

		public string ConnectionString
		{
			get
			{
				var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
				{
					DataSource = ServerName,
					InitialCatalog = DatabaseName,
					ApplicationName = applicationName,
					CurrentLanguage = currentLanguage
				};
				if (UseIntegratedSecurity)
				{
					sqlConnectionStringBuilder.IntegratedSecurity = UseIntegratedSecurity;
				}
				else
				{
					sqlConnectionStringBuilder.UserID = UserName;
					sqlConnectionStringBuilder.Password = Password;
				}

				return sqlConnectionStringBuilder.ConnectionString;
			}
		}

		public string ConnectionStringToMaster
		{
			get
			{
				var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
				{
					DataSource = ServerName,
					InitialCatalog = DatabaseHelper.MasterDatabaseName,
					CurrentLanguage = currentLanguage,
					ApplicationName = applicationName
				};
				if (UseIntegratedSecurity)
				{
					sqlConnectionStringBuilder.IntegratedSecurity = UseIntegratedSecurity;
				}
				else
				{
					sqlConnectionStringBuilder.UserID = UserName;
					sqlConnectionStringBuilder.Password = Password;
				}


				return sqlConnectionStringBuilder.ConnectionString;
			}
		}

		public string ConnectionStringAppLogOn(string databaseName)
		{
			{
				var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
				{
					DataSource = ServerName,
					InitialCatalog = databaseName,
					CurrentLanguage = currentLanguage,
					ApplicationName = applicationName
				};
				if (UseIntegratedSecurity)
				{
					sqlConnectionStringBuilder.IntegratedSecurity = UseIntegratedSecurity;
				}
				else
				{
					sqlConnectionStringBuilder.UserID = AppUserName;
					sqlConnectionStringBuilder.Password = AppUserPassword;
				}

				return sqlConnectionStringBuilder.ConnectionString;
			}
		}
	}
}