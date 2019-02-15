using System.Linq;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Support.Library;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Wfm.Administration.Core
{
	public class RestorePersonInfoOnDetach
	{
		private readonly LoadAllPersonInfos _loadAllPersonInfos;
		private readonly IInstallationEnvironment _installationEnvironment;

		public RestorePersonInfoOnDetach(LoadAllPersonInfos loadAllPersonInfos, IInstallationEnvironment installationEnvironment)
		{
			_loadAllPersonInfos = loadAllPersonInfos;
			_installationEnvironment = installationEnvironment;
		}

		public void Restore(Tenant tenant)
		{
			var personInfos = _loadAllPersonInfos.PersonInfos().Where(p => p.Tenant.Name.Equals(tenant.Name)).ToList();

			var helper = new DatabaseHelper(tenant.DataSourceConfiguration.ApplicationConnectionString, DatabaseType.TeleoptiCCC7, _installationEnvironment);
			
			using (var tenantUowManager =
				TenantUnitOfWorkManager.Create(tenant.DataSourceConfiguration.ApplicationConnectionString))
			{
				using (tenantUowManager.EnsureUnitOfWorkIsStarted())
				{
			   		var allTenants = new LoadAllTenants(tenantUowManager).Tenants().ToList();
					if(allTenants.Count != 1)
						return;
					var oldTenant = allTenants.First();
					helper.RemoveOldPersonInfos();

					foreach (var personInfo in personInfos)
					{
						var pInfo = new PersonInfo(oldTenant, personInfo.Id);
						pInfo.SetIdentity(personInfo.Identity);
						pInfo.SetApplicationLogonCredentials(null, personInfo.ApplicationLogonInfo.LogonName, null, null);
						pInfo.ApplicationLogonInfo.SetEncryptedPasswordIfLogonNameExistButNoPassword(personInfo.ApplicationLogonInfo.LogonPassword);
						tenantUowManager.CurrentSession().Save(pInfo);
					}
				}
			}

		}
	}
}