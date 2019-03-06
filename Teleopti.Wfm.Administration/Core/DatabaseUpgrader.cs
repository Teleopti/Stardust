using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Support.Library;

namespace Teleopti.Wfm.Administration.Core
{
	public class DatabaseUpgrader
	{
		private readonly DatabasePatcher _databasePatcher;
		private readonly IDbPathProvider _pathProvider;
		private readonly IConfigReader _config;

		public DatabaseUpgrader(DatabasePatcher databasePatcher, IDbPathProvider pathProvider, IConfigReader config)
		{
			_databasePatcher = databasePatcher;
			_pathProvider = pathProvider;
			_config = config;
		}

		public int Upgrade(string server, string database, DatabaseType type, string adminUserName, string adminPassword, bool useIntegratedSecurity, string appUser, string appPassword, bool permissionMode, string tenant, int tenantId)
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
			_databasePatcher.SetLogger(new TenantLogger(tenant, tenantId, _config));
			return _databasePatcher.Run(command);
		}
	}
}