namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class PersistPersonInfoFake : IPersistPersonInfo
	{
		public void Persist(PersonInfo personInfo)
		{
			LastPersist = personInfo;
		}

		public PersonInfo LastPersist { get; private set; }
	}
}