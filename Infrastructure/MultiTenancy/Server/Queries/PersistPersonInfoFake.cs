using System;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersistPersonInfoFake : IPersistPersonInfo
	{
		public void Persist(PersonInfo personInfo)
		{
			LastPersist = personInfo;
			if (personInfo.ApplicationLogonInfo.LogonName == "existingId@teleopti.com")
			{
				throw new DuplicateApplicationLogonNameException(Guid.NewGuid());
			}
		}

		public PersonInfo LastPersist { get; private set; }
	}
}