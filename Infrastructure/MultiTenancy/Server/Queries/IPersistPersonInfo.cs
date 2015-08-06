namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IPersistPersonInfo
	{
		void Persist(PersonInfo personInfo, string logonName, bool noNewPassword);
	}
}