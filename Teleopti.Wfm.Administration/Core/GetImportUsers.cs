using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public interface IGetImportUsers
	{
		ConflictModel GetConflictionUsers(string importConnectionString, string userPrefix);
	}

	public class GetImportUsers : IGetImportUsers
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public GetImportUsers(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public ConflictModel GetConflictionUsers(string importConnectionString, string userPrefix)
		{
			//don't do the check if not entered Tenant
			if (string.IsNullOrEmpty(userPrefix))
				return new ConflictModel();

			var mainUsers = _currentTenantSession.CurrentSession()
				.GetNamedQuery("loadAll")
				.List<PersonInfo>();

			IList<PersonInfo> importUsers = null;
			using (var tenantUowManager = TenantUnitOfWorkManager.Create(importConnectionString))
			{
				using (tenantUowManager.EnsureUnitOfWorkIsStarted())
				{
					importUsers = new LoadAllPersonInfos(tenantUowManager).PersonInfos().ToList();
				}
			}

			var conflicting = new HashSet<ImportUserModel>();
			var notConflicting = new HashSet<ImportUserModel>();
			foreach (var importUser in importUsers)
			{
				var checkedImport = checkConflict(mainUsers, importUser, userPrefix);
				if (checkedImport == null)
					continue;
				var importUserModel = new ImportUserModel
				{
					AppLogon = checkedImport.AppLogon,
					Identity = checkedImport.Identity,
					AppPassword = importUser.ApplicationLogonInfo.LogonPassword,
					PersonId = importUser.Id
				};
				if (checkedImport.Conflicted)
					conflicting.Add(importUserModel);
				else
					notConflicting.Add(importUserModel);
			}
			var retModel = new ConflictModel
			{
				NumberOfConflicting = conflicting.Count,
				NumberOfNotConflicting = notConflicting.Count,
				ConflictingUserModels = conflicting,
				NotConflicting = notConflicting
			};

			return retModel;
		}

		private CheckConflictResult checkConflict(IList<PersonInfo> allOldOnes, PersonInfo toImport, string tenant)
		{
			var logonName = toImport.ApplicationLogonInfo.LogonName;
			var identity = toImport.Identity;
			var id = toImport.Id;
			PersonInfo conflictId = allOldOnes.FirstOrDefault(x => x.Id.Equals(id));
			if (conflictId != null)
				return null;
			if (string.IsNullOrEmpty(toImport.Identity) && string.IsNullOrEmpty(toImport.ApplicationLogonInfo.LogonName))
				return null;

			bool conflicting = false;
			if (logonName != null)
			{
				PersonInfo conflictLogonName = allOldOnes.FirstOrDefault(
					x => x.ApplicationLogonInfo.LogonName != null && x.ApplicationLogonInfo.LogonName.Equals(logonName, StringComparison.OrdinalIgnoreCase));
				if (conflictLogonName != null)
				{
					conflicting = true;
					logonName = tenant + logonName;
					//hopefully no conflict now
					conflictLogonName =
					allOldOnes.FirstOrDefault(
						x => x.ApplicationLogonInfo.LogonName != null && x.ApplicationLogonInfo.LogonName.Equals(logonName, StringComparison.OrdinalIgnoreCase));
					if (conflictLogonName != null)
					{
						int suffix = 0;
						do
						{
							suffix++;
							logonName = logonName + suffix;
							conflictLogonName = allOldOnes.FirstOrDefault(
							x => x.ApplicationLogonInfo.LogonName != null && x.ApplicationLogonInfo.LogonName.Equals(logonName, StringComparison.OrdinalIgnoreCase));

						} while (conflictLogonName != null);
					}
				}
			}

			if (identity != null)
			{
				PersonInfo conflictIdentity = allOldOnes.FirstOrDefault(x => x.Identity != null && x.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase));
				if (conflictIdentity != null)
				{
					conflicting = true;
					identity = tenant + identity;
					//hopefully no conflict now
					conflictIdentity =
					allOldOnes.FirstOrDefault(x => x.Identity != null && x.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase));
					if (conflictIdentity != null)
					{
						int suffix = 0;
						do
						{
							suffix++;
							identity = identity + suffix;
							conflictIdentity = allOldOnes.FirstOrDefault(x => x.Identity != null && x.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase));

						} while (conflictIdentity != null);
					}
				}
			}

			return new CheckConflictResult { Conflicted = conflicting, AppLogon = logonName, Identity = identity };

		}
	}

	internal class CheckConflictResult
	{
		public string AppLogon { get; set; }
		public string Identity { get; set; }
		public bool Conflicted { get; set; }
	}

}