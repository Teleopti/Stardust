using System;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersistPersonInfoFake : IPersistPersonInfo
	{
		private bool _rollbacked = false;

		public string Persist(GenericPersistApiCallActionObj genericPersistApiCallAction)
		{
			LastPersist = genericPersistApiCallAction.PersonInfo;
			if (genericPersistApiCallAction.PersonInfo.ApplicationLogonInfo.LogonName == "existingId@teleopti.com")
			{
				if (genericPersistApiCallAction.ThrowOnError)
				{
					throw new DuplicateApplicationLogonNameException(Guid.NewGuid());
				}
				else
				{
					return "ErrorMessageString";
				}
			}
			return null;
		}

		public string PersistApplicationLogonName(AppLogonChangeActionObj appLogonChangeActionObj)
		{
			LastPersist = appLogonChangeActionObj.PersonInfo;
			if (appLogonChangeActionObj.PersonInfo.ApplicationLogonInfo.LogonName == "existingId@teleopti.com")
			{
				if (appLogonChangeActionObj.ThrowOnError)
				{
					throw new DuplicateApplicationLogonNameException(Guid.NewGuid());
				}
				else
				{
					return "ErrorMessageString";
				}
			}
			return null;
		}

		public string PersistIdentity(IdentityChangeActionObj identityChangeActionObj)
		{
			LastPersist = identityChangeActionObj.PersonInfo;
			if (identityChangeActionObj.PersonInfo.Identity == "existingId@teleopti.com")
			{
				if (identityChangeActionObj.ThrowOnError)
				{
					throw new DuplicateIdentityException(Guid.NewGuid());
				}
				else
				{
					return "ErrorMessageString";
				}
			}
			return null;
		}

		public PersonInfo LastPersist { get; private set; }

		public void RollBackPersonInfo(Guid personInfoId, string tenantName)
		{
			_rollbacked = true;
		}

		public bool RollBacked => _rollbacked;
	}
}