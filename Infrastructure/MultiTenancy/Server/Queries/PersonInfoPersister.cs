using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersonInfoPersister : IPersonInfoPersister
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public PersonInfoPersister(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public void Persist(PersonInfo personInfo)
		{
			var session = _currentTenantSession.CurrentSession();
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

		public bool ValidateApplicationLogonNameIsUnique(PersonInfo personInfo)
		{
			var session = _currentTenantSession.CurrentSession();
			var existing = session.GetNamedQuery("applicationLogonNameUQCheck")
				.SetGuid("id", personInfo.Id)
				.SetString("applicationLogonName", personInfo.ApplicationLogonInfo.LogonName)
				.UniqueResult<PersonInfo>();
			return existing == null;
		}


		public bool ValidateIdenitityIsUnique(PersonInfo personInfo)
		{
			var session = _currentTenantSession.CurrentSession();
			var existing = session.GetNamedQuery("identityUQCheck")
				.SetGuid("id", personInfo.Id)
				.SetString("identity", personInfo.Identity)
				.UniqueResult<PersonInfo>();
			return existing == null;
		}
	}
}