using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IPersonInfoPersister
	{
		void Persist(PersonInfo personInfo);
		bool ValidateApplicationLogonNameIsUnique(PersonInfo personInfo);
		bool ValidateIdenitityIsUnique(PersonInfo personInfo);
		void PersistIdentity(PersonInfo personInfo);
		void PersistApplicationLogonName(PersonInfo personInfo);
	}

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

		public void PersistIdentity(PersonInfo personInfo)
		{
			var session = _currentTenantSession.CurrentSession();
			var currentPersonInfo = session.Get<PersonInfo>(personInfo.Id);

			if (currentPersonInfo == null)
			{
				session.Save(personInfo);
			}
			else
			{
				if (string.IsNullOrWhiteSpace(personInfo.Identity))
				{
					currentPersonInfo.SetIdentity(null);
				}
				else
				{
					currentPersonInfo.SetIdentity(personInfo.Identity);
				}

				if (string.IsNullOrWhiteSpace(currentPersonInfo.ApplicationLogonInfo.LogonName) &&
					string.IsNullOrWhiteSpace(currentPersonInfo.Identity))
				{
					session.Delete(currentPersonInfo);
				}
				else
				{
					session.Merge(currentPersonInfo);
				}
			}
		}

		public void PersistApplicationLogonName(PersonInfo personInfo)
		{
			var session = _currentTenantSession.CurrentSession();
			var currentPersonInfo = session.Get<PersonInfo>(personInfo.Id);

			if (currentPersonInfo == null)
			{
				session.Save(personInfo);
			}
			else
			{
				if (string.IsNullOrWhiteSpace(personInfo.ApplicationLogonInfo.LogonName))
				{
					currentPersonInfo.ApplicationLogonInfo.ClearLogonInfo();
				}
				else
				{
					currentPersonInfo.ApplicationLogonInfo.SetLogonName(personInfo.ApplicationLogonInfo.LogonName);
				}

				if (string.IsNullOrWhiteSpace(currentPersonInfo.ApplicationLogonInfo.LogonName) &&
					string.IsNullOrWhiteSpace(currentPersonInfo.Identity))
				{
					session.Delete(currentPersonInfo);
				}
				else
				{
					session.Merge(currentPersonInfo);
				}
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