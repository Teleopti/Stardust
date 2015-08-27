using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public class Import
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly PersistTenant _persistTenant;

		public Import(ICurrentTenantSession currentTenantSession, PersistTenant persistTenant)
		{
			_currentTenantSession = currentTenantSession;
			_persistTenant = persistTenant;
		}

		public ImportTenantResultModel Execute(ImportDatabaseModel model, ConflictModel conflictModel)
		{
			var newTenant = new Tenant(model.Tenant);
			newTenant.DataSourceConfiguration.SetApplicationConnectionString(model.ConnStringAppDatabase);
			newTenant.DataSourceConfiguration.SetAnalyticsConnectionString(model.ConnStringAnalyticsDatabase);
			_persistTenant.Persist(newTenant);

			saveToDb(conflictModel.NotConflicting, newTenant);
			saveToDb(conflictModel.ConflictingUserModels, newTenant);
			
			return new ImportTenantResultModel
			{
				Success = true,
				Message = string.Format("Succesfully imported a new Tenant with {0} user.", conflictModel.NumberOfConflicting + conflictModel.NumberOfNotConflicting)
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