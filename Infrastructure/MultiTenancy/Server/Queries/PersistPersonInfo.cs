﻿using System;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Aspects;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
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
		
		[TenantAudit]
		public virtual string Persist(PersonInfo personInfo, PersistActionIntent actionIntent, bool throwOnError = true)
		{
			var res = ValidatePersonInfo(personInfo, throwOnError);

			if (!string.IsNullOrEmpty(res))
			{
				return res;
			}

			_personInfoPersister.Persist(personInfo);
			return null;
		}

		[TenantAudit]
		public virtual string PersistIdentity(PersonInfo personInfo, PersistActionIntent actionIntent, bool throwOnError = true)
		{
			return persistHelper(_personInfoPersister.PersistIdentity, personInfo, throwOnError);
		}

		[TenantAudit]
		public virtual string PersistApplicationLogonName(PersonInfo personInfo, PersistActionIntent actionIntent, bool throwOnError = true)
		{
			return persistHelper(_personInfoPersister.PersistApplicationLogonName, personInfo, throwOnError);
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
}