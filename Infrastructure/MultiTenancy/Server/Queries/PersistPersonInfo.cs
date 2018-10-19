using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Aspects;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersistPersonInfo : IPersistPersonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly IPersonInfoPersister _personInfoPersister;

		public PersistPersonInfo(ICurrentTenantSession currentTenantSession
			,IPersonInfoPersister personInfoPersister)
		{
			_currentTenantSession = currentTenantSession;
			_personInfoPersister = personInfoPersister;
		}
		
		[TenantAudit(PersistActionIntent.GenericPersistApiCall)]
		//public virtual string Persist(PersonInfo personInfo, bool throwOnError = true)
		public virtual string Persist(GenericPersistApiCallActionObj genericPersistAuditAction)
		{
			var res = ValidatePersonInfo(genericPersistAuditAction.PersonInfo, genericPersistAuditAction.ThrowOnError);

			if (!string.IsNullOrEmpty(res))
			{
				return res;
			}

			_personInfoPersister.Persist(genericPersistAuditAction.PersonInfo);
			return null;
		}


		[TenantAudit(PersistActionIntent.IdentityChange)]
		public virtual string PersistIdentity(IdentityChangeActionObj identityChangeActionObj)
		{
			return persistHelper(_personInfoPersister.PersistIdentity, identityChangeActionObj.PersonInfo, identityChangeActionObj.ThrowOnError);
		}

		[TenantAudit(PersistActionIntent.AppLogonChange)]
		public virtual string PersistApplicationLogonName(AppLogonChangeActionObj appLogonChangeActionObj)
		{
			return persistHelper(_personInfoPersister.PersistApplicationLogonName, appLogonChangeActionObj.PersonInfo, appLogonChangeActionObj.ThrowOnError);
		}

		private string persistHelper(Action<PersonInfo> persist, PersonInfo personInfo, bool throwOnError = true)
		{
			var res = ValidatePersonInfo(personInfo, throwOnError);

			if (!string.IsNullOrEmpty(res))
			{
				return res;
			}

			persist(personInfo);

			return null;
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

	public static class IntentTest
	{
		public const string NotSet = "NotSet";
	}
	public enum PersistActionIntent
	{
		NotSet,
		AppLogonChange,
		AppPasswordChange,
		IdentityChange,
		GenericPersistApiCall
	}

	public class PersistPersonInfoWithAuditTrail : IPersistPersonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly IPersonInfoPersister _personInfoPersister;

		public PersistPersonInfoWithAuditTrail(ICurrentTenantSession currentTenantSession
			, IPersonInfoPersister personInfoPersister)
		{
			_currentTenantSession = currentTenantSession;
			_personInfoPersister = personInfoPersister;
		}

		//[TenantAudit(PersistActionIntent.GenericPersistApiCall)]
		[AuditTrail]
		public virtual string Persist(GenericPersistApiCallActionObj genericPersistAuditAction)
		{
			var res = ValidatePersonInfo(genericPersistAuditAction.PersonInfo, genericPersistAuditAction.ThrowOnError);

			if (!string.IsNullOrEmpty(res))
			{
				return res;
			}

			_personInfoPersister.Persist(genericPersistAuditAction.PersonInfo);
			return null;
		}


		//[TenantAudit(PersistActionIntent.IdentityChange)]
		public virtual string PersistIdentity(IdentityChangeActionObj identityChangeActionObj)
		{
			return persistHelper(_personInfoPersister.PersistIdentity, identityChangeActionObj.PersonInfo, identityChangeActionObj.ThrowOnError);
		}

		//[TenantAudit(PersistActionIntent.AppLogonChange)]
		public virtual string PersistApplicationLogonName(AppLogonChangeActionObj appLogonChangeActionObj)
		{
			return persistHelper(_personInfoPersister.PersistApplicationLogonName, appLogonChangeActionObj.PersonInfo, appLogonChangeActionObj.ThrowOnError);
		}

		private string persistHelper(Action<PersonInfo> persist, PersonInfo personInfo, bool throwOnError = true)
		{
			var res = ValidatePersonInfo(personInfo, throwOnError);

			if (!string.IsNullOrEmpty(res))
			{
				return res;
			}

			persist(personInfo);

			return null;
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

	public class TenantAuditAction
	{
		public PersonInfo PersonInfo { get; set; }
		public bool ThrowOnError = true;
	}

	public class IdentityChangeActionObj : TenantAuditAction
	{

	}

	public class AppLogonChangeActionObj : TenantAuditAction
	{

	}

	public class GenericPersistApiCallActionObj : TenantAuditAction
	{

	}

	public class TenantAccessAuditContext : IHandleContextAction<GenericPersistApiCallActionObj>,
		IHandleContextAction<AppLogonChangeActionObj>, IHandleContextAction<IdentityChangeActionObj>
	{
		private readonly ITenantAuditPersister _tenantAuditPersister;
		private readonly ICurrentHttpContext _currentHttpContext;

		public TenantAccessAuditContext(ITenantAuditPersister tenantAuditPersister, ICurrentHttpContext currentHttpContext)
		{
			_tenantAuditPersister = tenantAuditPersister;
			_currentHttpContext = currentHttpContext;
		}

		public void Handle(GenericPersistApiCallActionObj command)
		{
			AuditPersist(command.PersonInfo, PersistActionIntent.GenericPersistApiCall);
		}

		public void Handle(AppLogonChangeActionObj command)
		{
			AuditPersist(command.PersonInfo, PersistActionIntent.AppLogonChange);
		}

		public void Handle(IdentityChangeActionObj command)
		{
			AuditPersist(command.PersonInfo, PersistActionIntent.IdentityChange);
		}

		private void AuditPersist(PersonInfo personInfo, PersistActionIntent intent)
		{
			var tenantUser = _currentHttpContext.Current().Items[WebTenantAuthenticationConfiguration.PersonInfo] as PersonInfo;

			var data = Newtonsoft.Json.JsonConvert.SerializeObject(new
			{
				Identity = personInfo.Identity ?? string.Empty,
				AppLogonName = personInfo.ApplicationLogonInfo.LogonName ?? string.Empty,
				AppLogonPwd = string.IsNullOrEmpty(personInfo.ApplicationLogonInfo.LogonPassword) ? string.Empty : "<pwd-hash-set>"
			});

			var ta = new TenantAudit(tenantUser.Id, personInfo.Id, intent.ToString(), data);
			_tenantAuditPersister.Persist(ta);
		}
	}

}