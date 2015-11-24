using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public class Import
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly IGetImportUsers _getImportUsers;
		private readonly PersistTenant _persistTenant;
		private readonly TenantUpgrader _tenantUpgrader;

		public Import(ICurrentTenantSession currentTenantSession, IGetImportUsers getImportUsers, PersistTenant persistTenant, TenantUpgrader tenantUpgrader)
		{
			_currentTenantSession = currentTenantSession;
			_getImportUsers = getImportUsers;
			_persistTenant = persistTenant;
			_tenantUpgrader = tenantUpgrader;
		}

		public ImportTenantResultModel Execute(string tenant, string connStringApp, string connStringAnal, string aggConnectionstring, string adminUser, string adminPassword)
		{
			var newTenant = new Tenant(tenant);
			newTenant.DataSourceConfiguration.SetApplicationConnectionString(connStringApp);
			newTenant.DataSourceConfiguration.SetAnalyticsConnectionString(connStringAnal);
			newTenant.DataSourceConfiguration.SetAggregationConnectionString(aggConnectionstring);
			_persistTenant.Persist(newTenant);

			//run upgrade every time because it can be other changes (sp, views in analytics for example)
			_tenantUpgrader.Upgrade(newTenant, adminUser, adminPassword, true, false);

			var conflicts = _getImportUsers.GetConflictionUsers(connStringApp, newTenant.Name);
			saveToDb(conflicts.NotConflicting, newTenant);
			saveToDb(conflicts.ConflictingUserModels, newTenant);
			
			return new ImportTenantResultModel
			{
				Success = true,
				TenantId = newTenant.Id,
				Message = string.Format("Succesfully imported a new Tenant with {0} user.", conflicts.NumberOfConflicting + conflicts.NumberOfNotConflicting)
			};
		}

		private void saveToDb(IEnumerable<ImportUserModel> userModels, Tenant newTenant)
		{
			foreach (var userModel in userModels)
			{
				var personInfo = new PersonInfo(newTenant, userModel.PersonId);
				personInfo.SetIdentity(userModel.Identity);
				personInfo.SetApplicationLogonCredentials(null, userModel.AppLogon, null);
				personInfo.ApplicationLogonInfo.SetEncryptedPasswordIfLogonNameExistButNoPassword(userModel.AppPassword);
				_currentTenantSession.CurrentSession().Save(personInfo);
			}
		}
	}
}