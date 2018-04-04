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

		public PersistPersonInfo(ICurrentTenantSession currentTenantSession, IPersonInfoPersister personInfoPersister)
		{
			_currentTenantSession = currentTenantSession;
			_personInfoPersister = personInfoPersister;
		}

		public void Persist(PersonInfo personInfo)
		{
			if (personInfo.Id == Guid.Empty)
				throw new ArgumentException("Missing explicitly set id on personInfo object.");

			var session = _currentTenantSession.CurrentSession();

			if (!string.IsNullOrEmpty(personInfo.ApplicationLogonInfo.LogonName))
			{
				//if already exists
				var isUnique = _personInfoPersister.ValidateApplicationLogonNameIsUnique(personInfo);
				if (!isUnique)
					throw new DuplicateApplicationLogonNameException(personInfo.Id);
			}
			if (!string.IsNullOrEmpty(personInfo.Identity))
			{
				//if already exists
				var isUnique = _personInfoPersister.ValidateIdenitityIsUnique(personInfo);
				if (!isUnique)
					throw new DuplicateIdentityException(personInfo.Id);
			}

			_personInfoPersister.Persist(personInfo);
		}

		public string PersistEx(PersonInfo personInfo)
		{
			var res = ValidatePersonInfo(personInfo);

			if (!string.IsNullOrEmpty(res))
			{
				return res;
			}

			_personInfoPersister.Persist(personInfo);
			return null;
		}

		private string ValidatePersonInfo(PersonInfo personInfo)
		{
			if (personInfo.Id == Guid.Empty)
				throw new ArgumentException("Missing explicitly set id on personInfo object.");

			if (!string.IsNullOrEmpty(personInfo.ApplicationLogonInfo.LogonName))
			{
				var isUnique = _personInfoPersister.ValidateApplicationLogonNameIsUnique(personInfo);
				if (!isUnique)
					return string.Format(Resources.ApplicationLogonExists, personInfo.ApplicationLogonInfo.LogonName);
			}
			if (!string.IsNullOrEmpty(personInfo.Identity))
			{
				var isUnique = _personInfoPersister.ValidateIdenitityIsUnique(personInfo);
				if (!isUnique)
					return string.Format(Resources.IdentityLogonExists, personInfo.Identity);
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
}