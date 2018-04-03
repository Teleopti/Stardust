using System;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

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
				var existing = session.GetNamedQuery("applicationLogonNameUQCheck")
					.SetGuid("id", personInfo.Id)
					.SetString("applicationLogonName", personInfo.ApplicationLogonInfo.LogonName)
					.UniqueResult<PersonInfo>();
				if (existing != null)
					throw new DuplicateApplicationLogonNameException(existing.Id);
			}
			if (!string.IsNullOrEmpty(personInfo.Identity))
			{
				//if already exists
				var existing = session.GetNamedQuery("identityUQCheck")
					.SetGuid("id", personInfo.Id)
					.SetString("identity", personInfo.Identity)
					.UniqueResult<PersonInfo>();
				if (existing != null)
					throw new DuplicateIdentityException(existing.Id);
			}

			//throw 
			//else do next
			var oldPersonInfo = session.Get<PersonInfo>(personInfo.Id);
			if (oldPersonInfo == null)
			{
				session.Save(personInfo);
			}
			else
			{
				personInfo.ReuseTenantPassword(oldPersonInfo);
				// if we save an old we must reuse the old password if we get an logonname and no new password
				personInfo.ApplicationLogonInfo.SetEncryptedPasswordIfLogonNameExistButNoPassword(oldPersonInfo.ApplicationLogonInfo
					.LogonPassword);
				session.Merge(personInfo);
			}
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
					return "DuplicateApplicationLogonNameExistValidation"; 
			}
			if (!string.IsNullOrEmpty(personInfo.Identity))
			{
				var isUnique = _personInfoPersister.ValidateIdenitityIsUnique(personInfo);
				if (!isUnique)
					return "DuplicateIdenityLogonNameExistValidation"; 
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
	public interface IPersonInfoPersister
	{
		void Persist(PersonInfo personInfo);
		bool ValidateApplicationLogonNameIsUnique(PersonInfo personInfo);
		bool ValidateIdenitityIsUnique(PersonInfo personInfo);
	}
}