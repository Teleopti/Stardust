using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public class GetImportUsers
	{
		private readonly LoadAllPersonInfos _loadAllPersonInfos;
		private readonly ICurrentTenantSession _currentTenantSession;

		public GetImportUsers(LoadAllPersonInfos loadAllPersonInfos, ICurrentTenantSession currentTenantSession)
		{
			_loadAllPersonInfos = loadAllPersonInfos;
			_currentTenantSession = currentTenantSession;
		}

		public ConflictModel GetConflictionUsers(string importConnectionString)
		{
			var mainUsers = _currentTenantSession.CurrentSession()
				.GetNamedQuery("loadAll")
				.List<PersonInfo>();
				
				//_loadAllPersonInfos.PersonInfos().ToList();


			var tenantUowManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(importConnectionString);
			tenantUowManager.Start();
			var importUsers = new LoadAllPersonInfos(tenantUowManager).PersonInfos();
			tenantUowManager.CancelAndDisposeCurrent();
			var conflicting = new HashSet<PersonInfo>();
			foreach (var importUser in importUsers)
			{
				var logonName = importUser.ApplicationLogonInfo.LogonName;
				var identity = importUser.Identity;
				if (logonName != null)
				{
					var conflict = mainUsers.FirstOrDefault(x => x.ApplicationLogonInfo.LogonName != null && x.ApplicationLogonInfo.LogonName.Equals(logonName));
					if (conflict != null)
						conflicting.Add(conflict);
				}

				if (identity != null)
				{
					var iConflict = mainUsers.FirstOrDefault(x => x.Identity != null && x.Identity.Equals(identity));
					if (iConflict != null)
						conflicting.Add(iConflict);
				}
					
			}
			var retModel = new ConflictModel
			{
				NumberOfConflicting = conflicting.Count,
				NumberOfNotConflicting = mainUsers.Count - conflicting.Count,
				ConflictingUserModels = conflicting.Select(c => new ConflictingUserModel
				{
					AppLogon = c.ApplicationLogonInfo.LogonName,
					Identity = c.Identity
				})
			};

			return retModel;
		}
	}
}