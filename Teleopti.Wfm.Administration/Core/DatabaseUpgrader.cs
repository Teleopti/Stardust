using Teleopti.Ccc.DBManager.Library;

namespace Teleopti.Wfm.Administration.Core
{
	public class DatabaseUpgrader
	{
		private readonly DatabasePatcher _databasePatcher;
		private readonly IDbPathProvider _pathProvider;

		public DatabaseUpgrader(DatabasePatcher databasePatcher, IDbPathProvider pathProvider)
		{
			_databasePatcher = databasePatcher;
			_pathProvider = pathProvider;
		}

		public int Upgrade(string server, string database, DatabaseType type, string adminUserName, string adminPassword, string appUser, string appPassword, bool permissionMode)
		{
			var commands = new CommandLineArgument(new string[] {})
			{
				ServerName = server,
				DatabaseName = database,
				UserName = adminUserName,
				Password = adminPassword,
				appUserName = appUser,
				appUserPwd = appPassword,
				TargetDatabaseType = type,
				PathToDbManager = _pathProvider.GetDbPath(),
				PatchMode = true,
				PermissionMode = permissionMode
			};
			_databasePatcher.Logger = new TenantLogger();
         return _databasePatcher.Run(commands);

		}
	}
}