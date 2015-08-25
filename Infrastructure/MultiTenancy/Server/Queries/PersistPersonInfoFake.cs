namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersistPersonInfoFake : IPersistPersonInfo
	{
		public void Persist(PersonInfo personInfo)
		{
			LastPersist = personInfo;
			if (personInfo.ApplicationLogonInfo.LogonName == "existingId")
			{
				throw new DuplicateApplicationLogonNameException();
			}
		}

		public PersonInfo LastPersist { get; private set; }
	}
}