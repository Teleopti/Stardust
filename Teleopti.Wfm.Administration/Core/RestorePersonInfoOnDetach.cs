using System.Linq;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Support.Library;

namespace Teleopti.Wfm.Administration.Core
{
	public class RestorePersonInfoOnDetach
	{
		private readonly LoadAllPersonInfos _loadAllPersonInfos;

		public RestorePersonInfoOnDetach(LoadAllPersonInfos loadAllPersonInfos)
		{
			_loadAllPersonInfos = loadAllPersonInfos;
		}

		public void Restore(Tenant tenant)
		{
			var personInfos = _loadAllPersonInfos.PersonInfos().Where(p => p.Tenant.Name.Equals(tenant.Name)).ToList();

			var helper = new DatabaseHelper(tenant.DataSourceConfiguration.ApplicationConnectionString, DatabaseType.TeleoptiCCC7);
			
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