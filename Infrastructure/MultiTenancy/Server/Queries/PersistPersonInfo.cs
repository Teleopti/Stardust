using System;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersistPersonInfo : IPersistPersonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public PersistPersonInfo(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
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
				if(existing != null)
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
				personInfo.ApplicationLogonInfo.SetEncryptedPasswordIfLogonNameExistButNoPassword(oldPersonInfo.ApplicationLogonInfo.LogonPassword);
				session.Merge(personInfo);
			}
		}
	}
}