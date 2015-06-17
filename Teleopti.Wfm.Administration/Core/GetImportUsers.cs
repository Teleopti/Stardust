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
		private readonly ICurrentTenantSession _currentTenantSession;

		public GetImportUsers(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public ConflictModel CheckConflicting(string importConnectionString, string userPrefix)
		{
			//don't do the check if not entered Tenant
			if(string.IsNullOrEmpty(userPrefix))
				return new ConflictModel();

			var mainUsers = _currentTenantSession.CurrentSession()
				.GetNamedQuery("loadAll")
				.List<PersonInfo>();

			var tenantUowManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(importConnectionString);
			tenantUowManager.Start();
			var importUsers = new LoadAllPersonInfos(tenantUowManager).PersonInfos().ToList();
			tenantUowManager.CancelAndDisposeCurrent();
			var conflicting = new HashSet<ImportUserModel>();
			var notConflicting = new HashSet<PersonInfo>();
			foreach (var importUser in importUsers)
			{
				var checkedImport = checkConflict(mainUsers, importUser, userPrefix);
				var importUserModel = new ImportUserModel
				{
					AppLogon = checkedImport.AppLogon,
					Identity = checkedImport.Identity,
					{
					PersonId = importUser.Id
						continue;
					}
					conflicting.Add(importUserModel);
				else
					notConflicting.Add(importUserModel);
			}
			var retModel = new ConflictModel
			{
				NumberOfConflicting = conflicting.Count,
				NumberOfNotConflicting = importUsers.Count - conflicting.Count,
				ConflictingUserModels = conflicting,
				NotConflicting = notConflicting
			};

			return retModel;
		}

		private CheckConflictResult checkConflict(IList<PersonInfo> allOldOnes, PersonInfo toImport, string tenant)
		{
			var logonName = toImport.ApplicationLogonInfo.LogonName;
			var identity = toImport.Identity;
					{
			if (logonName != null)
						continue;
					}
					x => x.ApplicationLogonInfo.LogonName != null && x.ApplicationLogonInfo.LogonName.Equals(logonName));
				if (conflictLogonName != null)
				{
					conflicting = true;
					logonName = tenant + logonName;
					//hopefully no conflict now
					conflictLogonName =
					allOldOnes.FirstOrDefault(
						x => x.ApplicationLogonInfo.LogonName != null && x.ApplicationLogonInfo.LogonName.Equals(logonName));
					if (conflictLogonName != null)
					{
						int suffix = 0;
						do
						{
							suffix++;
							logonName = logonName + suffix;
							conflictLogonName = allOldOnes.FirstOrDefault(
							x => x.ApplicationLogonInfo.LogonName != null && x.ApplicationLogonInfo.LogonName.Equals(logonName));

						} while (conflictLogonName != null);
					}

				notConflicting.Add(importUser);
				}
			}

			if (identity != null)
			{
				PersonInfo conflictIdentity = allOldOnes.FirstOrDefault(x => x.Identity != null && x.Identity.Equals(identity));
				NumberOfNotConflicting = importUsers.Count - conflicting.Count,
				{
					conflicting = true;
					identity = tenant + identity;
				}),
				NotConflicting = notConflicting.Select(c => new NotConflictingUserModel
				{
					PersonId = c.Id,
					AppLogon = c.ApplicationLogonInfo.LogonName,
					Password = c.ApplicationLogonInfo.LogonPassword,
					Identity = c.Identity
				}),
							suffix++;
							identity = identity + suffix;
							conflictIdentity = allOldOnes.FirstOrDefault(x => x.Identity != null && x.Identity.Equals(identity));

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

