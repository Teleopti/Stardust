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

		public int Upgrade(string server, string database, DatabaseType type, string adminUserName, string adminPassword,bool useIntegratedSecurity, string appUser, string appPassword, bool permissionMode, string tenant, int tenantId)
		{
			var command = new PatchCommand
			{
				ServerName = server,
				DatabaseName = database,
				UserName = adminUserName,
				Password = adminPassword,
				AppUserName = appUser,
				AppUserPassword = appPassword,
				DatabaseType = type,
				DbManagerFolderPath = _pathProvider.GetDbPath(),
				UpgradeDatabase = true,
				CreatePermissions = permissionMode,
				UseIntegratedSecurity = useIntegratedSecurity
			};
			_databasePatcher.SetLogger(new TenantLogger(tenant, tenantId));
         return _databasePatcher.Run(command);

		}
	}
}