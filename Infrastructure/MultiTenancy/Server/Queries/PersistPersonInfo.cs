using System;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersistPersonInfo : IPersistPersonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly IPersonInfoPersister _personInfoPersister;
		private readonly ITenantAuditPersister _tenantAuditPersister;
		private readonly ICurrentTenantUser _currentTenantUser;

		public PersistPersonInfo(ICurrentTenantSession currentTenantSession
			,IPersonInfoPersister personInfoPersister
			,ITenantAuditPersister tenantAuditPersister
			,ICurrentTenantUser currentTenantUser)
		{
			_currentTenantSession = currentTenantSession;
			_personInfoPersister = personInfoPersister;
			_tenantAuditPersister = tenantAuditPersister;
			_currentTenantUser = currentTenantUser;
		}

		public string Persist(PersonInfo personInfo, PersistActionIntent actionIntent, bool throwOnError = true)
		{
			/// Make ccc (aspect) of this later
			AuditPersist(personInfo, actionIntent);
			/// 
			var res = ValidatePersonInfo(personInfo, throwOnError);

			if (!string.IsNullOrEmpty(res))
			{
				return res;
			}

			_personInfoPersister.Persist(personInfo);
			return null;
		}

		private void AuditPersist(PersonInfo personInfo, PersistActionIntent intent)
		{
			// break out inte aspect
			var tenantUser = _currentTenantUser.CurrentUser();
			var data = Newtonsoft.Json.JsonConvert.SerializeObject(new
			{
				Identity = personInfo.Identity,
				AppLogonName = personInfo.ApplicationLogonInfo.LogonName,
				AppLogonPwd = string.IsNullOrEmpty(personInfo.ApplicationLogonInfo.LogonPassword) ? string.Empty : "<pwd-hash-set>"
			});
			
			var ta = new TenantAudit(tenantUser.Id, personInfo.Id, intent.ToString(), string.Empty, data);
			_tenantAuditPersister.Persist(ta);
		}

		private string ValidatePersonInfo(PersonInfo personInfo, bool throwOnError = false)
		{
			if (personInfo.Id == Guid.Empty)
				throw new ArgumentException("Missing explicitly set id on personInfo object.");

			if (!string.IsNullOrEmpty(personInfo.ApplicationLogonInfo.LogonName))
			{
				var isUnique = _personInfoPersister.ValidateApplicationLogonNameIsUnique(personInfo);
				if (!isUnique)
				{
					if (throwOnError) throw new DuplicateApplicationLogonNameException(personInfo.Id);
					return string.Format(Resources.ApplicationLogonExists, personInfo.ApplicationLogonInfo.LogonName);
				}
			}
			if (!string.IsNullOrEmpty(personInfo.Identity))
			{
				var isUnique = _personInfoPersister.ValidateIdenitityIsUnique(personInfo);
				if (!isUnique)
				{
					if (throwOnError) throw new DuplicateIdentityException(personInfo.Id);
					return string.Format(Resources.IdentityLogonExists, personInfo.Identity);
				}
			}

			return null;
		}

		public void RollBackPersonInfo(Guid personInfoId, string tenantName)
		{
			var session = _currentTenantSession.CurrentSession();
			var personInfo = session.Get<PersonInfo>(personInfoId);
			if (personInfo != null && personInfo.Tenant.Name == tenantName)
			{
				session.Delete(personInfo);
			}
		}
	}

	public enum PersistActionIntent
	{
		NotSet,
		AppLogonChange,
		AppPasswordChange,
		IdentityChange,
		GenericPersistApiCall
	}
}