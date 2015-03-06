namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IPersistPersonInfo
	{
		void Persist(PersonInfo personInfo);
	}
}