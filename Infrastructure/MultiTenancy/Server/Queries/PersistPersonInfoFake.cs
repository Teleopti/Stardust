using System;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersistPersonInfoFake : IPersistPersonInfo
	{
		private bool _rollbacked = false;

		public string Persist(PersonInfo personInfo, PersistActionIntent intent, bool throwOnError)
		{
			LastPersist = personInfo;
			if (personInfo.ApplicationLogonInfo.LogonName == "existingId@teleopti.com")
			{
				if (throwOnError)
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

		public PersonInfo LastPersist { get; private set; }

		public void RollBackPersonInfo(Guid personInfoId, string tenantName)
		{
			_rollbacked = true;
		}

		public bool RollBacked => _rollbacked;
	}
}